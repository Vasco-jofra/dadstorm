using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using DadStormServices;
using ReplicaProcess.Routing;

namespace ReplicaProcess
{

    public sealed class GroupManager
    {
        private IList<IReplica> AllMembers { get; }
        public IList<IReplica> AliveMembers { get; }
        private IDictionary<DadTuple, Queue<int>> Shared { get; }
        public IReplica LocalReplica { get; } // TODO: fix, should be private
        private IDictionary<DadTupleId, IList<DadTuple>> Processed { get; }
        private ISet<DadTupleId> Delivered { get; }
        private ISet<DadTupleId> Deleted { get; }
        private MonitoringProcess MonitoringProcess { get; }
        private ISet<DadTuple> NoOwner { get; }
        private ITupleRoutingStrategy LocalRoutingStrategy { get; }

        public GroupManager(IList<IReplica> allMembers, IReplica localReplica, MonitoringProcess monitoringProcess, ITupleRoutingStrategy localRoutingStrategy)
        {
            AllMembers = new List<IReplica>(allMembers);
            AliveMembers = new List<IReplica>(allMembers);
            Shared = new Dictionary<DadTuple, Queue<int>>();
            LocalReplica = localReplica;
            Processed = new Dictionary<DadTupleId, IList<DadTuple>>();
            Delivered = new HashSet<DadTupleId>();
            Deleted = new HashSet<DadTupleId>();
            MonitoringProcess = monitoringProcess;
            NoOwner = new HashSet<DadTuple>();
            LocalRoutingStrategy = localRoutingStrategy;
        }

        public IEnumerable<IReplica> GetAlivePeers()
        {
            return AliveMembers.Where(r => r != LocalReplica);
        }

        public IEnumerable<IReplica> GetAllPeers() {
            return AllMembers.Where(r => r != LocalReplica);
        }

        public void SetPeerAsDead(IReplica deadPeer)
        {
            Debug.Assert(deadPeer != LocalReplica);
            lock (AliveMembers)
            {
                NoOwner.UnionWith(deadPeer.PopOwnedTuples());
                AliveMembers.Remove(deadPeer);
            }

            if (AmILeader() && NoOwner.Count > 0)
            {
                foreach (var freeTuple in NoOwner)
                {
                    Queue<int> interestedPeers;
                    if (Shared.TryGetValue(freeTuple, out interestedPeers))
                    {
                        var interestedAlivePeer = interestedPeers
                            .Select(id => AllMembers[id])
                            .Intersect(AliveMembers)
                            .FirstOrDefault();

                        if (interestedAlivePeer != null)
                        {
                            RMSend(r => r.SetOwner(freeTuple, interestedAlivePeer.ReplicaId));
                        }
                        else
                        {
                            var chosenPeerIdx = LocalRoutingStrategy.Route(freeTuple.Content, AliveMembers.Count);
                            var chosenPeer = AliveMembers[chosenPeerIdx];
                            RMSend(p => p.SetOwner(freeTuple, chosenPeer.ReplicaId));
                        }
                    }
                }
            }
        }

        public void StartMonitoring()
        {
            foreach (var alivePeer in GetAlivePeers())
            {
                MonitoringProcess.Subscribe(alivePeer, PeerDiesEvent);
            }
        }

        private void PeerDiesEvent(IReplica peer)
        {
            SetPeerAsDead(peer);
        }

        public void RMSend(Action<IReplica> action)
        {
            lock (AliveMembers)
            {
                foreach (var peer in GetAlivePeers())
                {
                    try
                    {
                        action(peer);
                    }
                    catch (Exception e)
                    {
                        // TODO
                        Console.WriteLine("[RMSend] Failed: {0}", e.Message);
                    }
                }
            }
            action(LocalReplica);
        }

        public bool IsLeader(int replicaId)
        {
            return GetLeader().ReplicaId == replicaId;
        }

        public void SetOwner(DadTuple tuple, int ownerId)
        {
            NoOwner.Remove(tuple);
            AllMembers[ownerId].AddOwnedTuple(tuple);
        }

        public void SaveShare(DadTuple tuple, int interestedId)
        {
            Queue<int> interestedSet;
            if (Shared.TryGetValue(tuple, out interestedSet))
            {
                interestedSet.Enqueue(interestedId);
            }
            else
            {
                interestedSet = new Queue<int>();
                interestedSet.Enqueue(interestedId);
                Shared.Add(tuple, interestedSet);
            }
        }

        public bool HasShare(DadTuple tuple)
        {
            return Shared.ContainsKey(tuple);
        }

        public void SaveProcessedTuples(DadTupleId upstreamId, List<DadTuple> processedTuples)
        {
            Debug.Assert(!Processed.ContainsKey(upstreamId));
            Debug.Assert(!Delivered.Contains(upstreamId));

            foreach (var processedTuple in processedTuples)
            {
                Debug.Assert(!Deleted.Contains(processedTuple.Id));
            }

            Processed.Add(upstreamId, processedTuples);
            // TODO: remove from shared?
        }

        public void AddDelivered(DadTupleId upstreamId)
        {
            Debug.Assert(!Delivered.Contains(upstreamId));

            Delivered.Add(upstreamId);
            // TODO: remove from shared?
        }

        public void DeleteProcessed(List<DadTupleId> outputIds)
        {
            Deleted.UnionWith(outputIds);

            foreach (var outputId in outputIds)
            {
                Processed.Remove(outputId);
            }
            // TODO: remove from shared?
        }

        public bool AmILeader()
        {
            return GetLeader() == LocalReplica;
        }

        public DadTuple GetInputTupleByUpstreamId(DadTupleId upstreamId)
        {
            return Shared.Keys.FirstOrDefault(t => t.Id.Equals(upstreamId));
        }

        private IReplica GetLeader()
        {
            return AliveMembers.Last();
        }

        private IReplica GetOwnerOfTuple(DadTupleId upstreamId)
        {
            return AliveMembers.SingleOrDefault(peer => peer.IsOwner(upstreamId));
        }
    }
}

using DadStormServices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReplicaProcess.Kernel;

namespace ReplicaProcess {
    public class ReplicaServices : MarshalByRefObject, IOperatorProcess {
        private bool HasStarted { get; set; }
        private bool IsFrozen { get; set; }
        private bool IsLogging { get; set; }
        private int Interval { get; set; }

        private TupleProcessor TupleProcessor { get; }

        private Operator operatorData { get; }
        public string OperatorID { get; }
        private int ReplicaID { get; }

        private GroupManager Group { get; }
        private MonitoringProcess MonitoringProcess { get; }

        public ReplicaServices(TupleProcessor tupleProcessor, GroupManager group, MonitoringProcess monitoringProcess, Operator operatorData, string operatorId, int replicaId)
        {
            HasStarted = false;
            IsFrozen = false;
            IsLogging = false; 
            Interval = 0;

            TupleProcessor = tupleProcessor;

            this.operatorData = operatorData;
            OperatorID = operatorId;
            ReplicaID = replicaId;

            Group = group;
            MonitoringProcess = monitoringProcess;
        }

        public void start() {
            if (HasStarted == false) {
                Console.WriteLine("Starting process!");
                HasStarted = true;
                TupleProcessor.Start();
                MonitoringProcess.Start();
                Group.StartMonitoring();
            }
            else {
                Console.WriteLine("Process already started!");
            }
        }

        public void interval(int milisseconds) {
            Console.WriteLine("Intervaling for {0}ms!", milisseconds);
            Interval = milisseconds;
        }

        public void crash() {
            Console.WriteLine("Crashing!");
            lock (Group)
            {
                System.Environment.Exit(0);
            }
        }

        public void status() {
            if (Group == null) {
                Console.WriteLine("[STATUS] Not yet connected to other replicas! Ask again later...");
                return;
            }

            Console.WriteLine("[STATUS] ------- START ------- ");

            Console.WriteLine(" --> [{0}({1})] ME! HasStarted: {2}; IsFrozen: {3}; Interval: {4}; IsLogging: {5}", OperatorID, ReplicaID, HasStarted, IsFrozen, Interval, IsLogging);
            foreach (IReplica rep in Group.GetAllPeers()) 
            {
                if (Group.AliveMembers.Contains(rep)) {
                    Console.WriteLine(" --> [{0}({1})] Looks healthy!", OperatorID, rep.ReplicaId);
                } else {
                    Console.WriteLine("  {1}. [{0}({1})] Looks DEAD!", OperatorID, rep.ReplicaId);
                }

            }

            Console.WriteLine("[STATUS] -------- END -------- ");
            Console.WriteLine();
        }

        public void freeze() {
            Console.WriteLine("Freezing!");
            IsFrozen = true;
            TupleProcessor.Freeze();
        }

        public void unfreeze() {
            Console.WriteLine("Unfreezing!");
            IsFrozen = false;
            TupleProcessor.Unfreeze();
        }

        public void startLogging() {
            Console.WriteLine("Started logging!");
            IsLogging = true;
            TupleProcessor.StartLogging();
        }

        public void stopLogging()
        {
            Console.WriteLine("Stopped logging!");
            IsLogging = false;
            TupleProcessor.StopLogging();
        }

        public void Ping() {
            // TODO: freeze on all methods
        }

        public void Flow(DadTuple tuple)
        {
            Console.WriteLine("[IN] Received tuple {0}.", tuple);
            if (Group.HasShare(tuple))
            {
                Console.WriteLine("[IN] Tuple {0} had been received before. Ignoring.", tuple);
                return;
            }

            Group.RMSend(p => p.Share(tuple, ReplicaID));

            // Sleep for a certain interval (0 by default)
            System.Threading.Thread.Sleep(Interval);
        }

        public void Share(DadTuple tuple, int senderId)
        {
            Console.WriteLine("[Share] Received share {0}.", tuple);

            if (Group.IsLeader(ReplicaID))
            {
                if (!Group.HasShare(tuple))
                {
                    Console.WriteLine("[LEADER] Setting the owner of tuple {0}.", tuple);
                    Group.RMSend(p => p.SetOwner(tuple, senderId));
                }
            }

            Group.SaveShare(tuple, senderId);
        }

        public void SetOwner(DadTuple tuple, int ownerId)
        {
            Group.SetOwner(tuple, ownerId);

            if (ownerId == ReplicaID)
            {
                Console.WriteLine("[OWNERSHIP] Ownership acquired for {0}.", tuple);
                TupleProcessor.Post(tuple);
            }
        }

        public void SaveProcessedTuples(DadTupleId oldId, List<DadTuple> processedTuples)
        {
            Console.WriteLine("[Services] Receiving processed tuples of {0}.", oldId);

            TupleProcessor.TupleWasProcessed(Group.GetInputTupleByUpstreamId(oldId));
            Group.SaveProcessedTuples(oldId, processedTuples);
        }

        public void DeliveredTuples(DadTupleId inputId, List<DadTupleId> outputIds)
        {
            Console.WriteLine("[Services] Receiving delivered status of {0} tuple(s) computed from {1}.", outputIds.Count, inputId);

            Group.AddDelivered(inputId);
            Group.DeleteProcessed(outputIds);
        }
    }
}

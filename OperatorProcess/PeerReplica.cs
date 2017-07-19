using System;
using System.Collections.Generic;
using DadStormServices;

namespace ReplicaProcess
{
    public class PeerReplica : IReplica
    {
        public string OperatorId { get; }
        public int ReplicaId { get; }
        protected IOperatorProcess Proxy { get; set; }
        private ISet<DadTuple> OwnedTuples { get; }

        public PeerReplica(string operatorId, int replicaId, IOperatorProcess proxy)
        {
            OperatorId = operatorId;
            ReplicaId = replicaId;
            Proxy = proxy;
            OwnedTuples = new HashSet<DadTuple>();
        }

        public void Share(DadTuple tuple, int senderId)
        {
            Proxy.Share(tuple, senderId);
        }

        public void SaveProcessedTuples(DadTupleId oldId, List<DadTuple> outTuples)
        {
            Proxy.SaveProcessedTuples(oldId, outTuples);
        }

        public void DeliveredTuples(DadTupleId inputId, List<DadTupleId> outputIds)
        {
            Proxy.DeliveredTuples(inputId, outputIds);
        }

        public void Ping()
        {
            Proxy.Ping();
        }

        public void SetOwner(DadTuple tuple, int ownerId)
        {
            Proxy.SetOwner(tuple, ownerId);
        }

        public void AddOwnedTuple(DadTuple tuple)
        {
            OwnedTuples.Add(tuple);
        }

        public IEnumerable<DadTuple> PopOwnedTuples()
        {
            var copy = new HashSet<DadTuple>(OwnedTuples);
            OwnedTuples.Clear();
            return copy;
        }

        public override string ToString()
        {
            return OperatorId + "(" + ReplicaId + ")";
        }
    }
}

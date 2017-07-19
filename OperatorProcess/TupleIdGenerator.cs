using DadStormServices;

namespace ReplicaProcess
{
    public class TupleIdGenerator
    {
        private long currentTupleId = 0;
        private readonly object nextTupleLock = new object();

        private string OperatorId { get; }
        private int ReplicaId { get; }

        public TupleIdGenerator(string operatorId, int replicaId)
        {
            OperatorId = operatorId;
            ReplicaId = replicaId;
        }

        public DadTupleId NextTupleId()
        {
            return new DadTupleId(OperatorId, ReplicaId, NextSequenceId());
        }

        private long NextSequenceId()
        {
            lock (nextTupleLock)
            {
                return currentTupleId++;
            }
        }
    }
}

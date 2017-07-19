using System.Collections.Generic;
using System.Linq;

namespace ReplicaProcess.Routing
{
    class HashingRoutingStrategy : AbstractRoutingStrategy
    {
        private int FieldIdx { get; }

        public HashingRoutingStrategy(int fieldIdx)
        {
            FieldIdx = fieldIdx;
        }

        protected override int RoutingAlgorithm(IList<string> tuple, int nReplicas)
        {
            return Hash(tuple[FieldIdx], nReplicas);
        }

        private int Hash(string field, int nReplicas)
        {
            return field.GetHashCode() % nReplicas;
        }
    }
}

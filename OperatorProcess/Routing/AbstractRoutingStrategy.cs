using System.Collections.Generic;

namespace ReplicaProcess.Routing
{
    public abstract class AbstractRoutingStrategy : ITupleRoutingStrategy
    {
        public int Route(IList<string> tuple, int nReplicas)
        {
            return RoutingAlgorithm(tuple, nReplicas);
        }

        protected abstract int RoutingAlgorithm(IList<string> tuple, int nReplicas);
    }
}

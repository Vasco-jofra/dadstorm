using System.Collections.Generic;

namespace ReplicaProcess.Routing
{
    public class PrimaryRoutingStrategy : AbstractRoutingStrategy
    {
        protected override int RoutingAlgorithm(IList<string> tuple, int nReplicas)
        {
            return 0;
        }
    }
}

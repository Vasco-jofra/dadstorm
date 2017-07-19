using System.Collections.Generic;

namespace ReplicaProcess.Routing
{
    public interface ITupleRoutingStrategy
    {
        int Route(IList<string> tuple, int nReplicas);
    }
}

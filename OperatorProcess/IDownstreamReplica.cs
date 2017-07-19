using System.Collections.Generic;
using DadStormServices;

namespace ReplicaProcess
{
    public interface IDownstreamReplica
    {
        bool Flow(DadTuple tuple, bool isLogging);
    }
}
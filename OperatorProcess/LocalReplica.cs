using DadStormServices;

namespace ReplicaProcess
{
    public class LocalReplica : PeerReplica
    {
        public LocalReplica(string operatorId, int replicaId, IOperatorProcess proxy) : base(operatorId, replicaId, proxy)
        {
        }

        public void SetProxy(IOperatorProcess proxy)
        {
            Proxy = proxy;
        }
    }
}
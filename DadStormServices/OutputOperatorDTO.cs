using System.Collections.Generic;

namespace DadStormServices
{
    public class OutputOperatorDTO
    {
        public List<string> ReplicasUrl { get; }
        public RoutingPolicy RoutingPolicy { get; }
        public int HashingField { get; }
        public string operatorId { get; }

        public OutputOperatorDTO(string operatorId, List<string> replicasUrl, RoutingPolicy routingPolicy, int hashingField)
        {
            this.operatorId = operatorId;
            ReplicasUrl = replicasUrl;
            RoutingPolicy = routingPolicy;
            HashingField = hashingField;
        }
    }
}

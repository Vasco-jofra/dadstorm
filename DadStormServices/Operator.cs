using System.Collections.Generic;
using Newtonsoft.Json;

namespace DadStormServices {
    public enum RoutingPolicy { Primary, Hashing, Random }

    public class Operator {
        public string id { get; }

        public List<string> inputs { get; }
        public List<string> inputFiles { get; }
        public List<OutputOperatorDTO> Outputs { get; }

        public int replicationFactor { get; }
        public RoutingPolicy routingPolicy { get; }
        public int HashingField { get; }
        public List<string> replicaURLs { get; }

        public string operatorSpec { get; }
        public List<string> operatorSpecArgs { get; }


        public Operator(string id, List<string> inputs, int replicationFactor, RoutingPolicy routingPolicy, int hashingField, List<string> replicaURLs, string operatorSpec, List<string> operatorSpecArgs) {
            this.id = id;
            this.inputs = inputs;

            this.replicationFactor = replicationFactor;
            this.routingPolicy = routingPolicy;
            this.HashingField = hashingField;
            this.replicaURLs = replicaURLs;

            this.operatorSpec = operatorSpec;
            this.operatorSpecArgs = operatorSpecArgs;

            Outputs = new List<OutputOperatorDTO>();
            inputFiles = new List<string>();
        }

        public string serialize() {
            return JsonConvert.SerializeObject(this);
        }

        public static Operator deserialize(string operatorJson) {
            return JsonConvert.DeserializeObject<Operator>(operatorJson);
        }
    }
}
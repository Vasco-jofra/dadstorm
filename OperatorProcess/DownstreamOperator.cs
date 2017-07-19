using System;
using System.Collections.Generic;
using DadStormServices;
using ReplicaProcess.Routing;

namespace ReplicaProcess
{
    public class DownstreamOperator
    {
        private string Id { get; }
        private List<IDownstreamReplica> AllReplicas { get; }
        private List<IDownstreamReplica> AliveReplicas { get; }
        private ITupleRoutingStrategy Router { get; }

        public DownstreamOperator(string id, List<IDownstreamReplica> allReplicas, ITupleRoutingStrategy router)
        {
            Id = id;
            AllReplicas = allReplicas;
            AliveReplicas = allReplicas;
            Router = router;
        }

        public void Flow(IList<DadTuple> tuples, bool isLogging)
        {
            foreach (var tuple in tuples)
            {
                Flow(tuple, isLogging);
            }
        }

        public void Flow(DadTuple tuple, bool isLogging)
        {
            var sent = false;

            while (!sent && AliveReplicas.Count > 0)
            {
                var chosenReplicaIdx = Router.Route(tuple.Content, AliveReplicas.Count);
                var chosenReplica = AliveReplicas[chosenReplicaIdx];

                sent = chosenReplica.Flow(tuple, isLogging);

                if (!sent)
                {
                    AliveReplicas.Remove(chosenReplica);
                }
            }

            if (!sent)
            {
                Console.WriteLine("[OUT] WARNING: Unable to send tuple to any replica of {0}.", Id);
            }
        }
    }
}

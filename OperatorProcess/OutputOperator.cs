using System;
using System.Collections.Generic;
using DadStormServices;
using ReplicaProcess.Routing;

namespace ReplicaProcess
{
    class OutputOperator
    {
        private string Id { get; }
        private List<IOperatorProcess> Replicas { get; }
        private ITupleRoutingStrategy Router { get; }

        public OutputOperator(string id, List<IOperatorProcess> replicas, ITupleRoutingStrategy router)
        {
            Id = id;
            Replicas = replicas;
            Router = router;
        }

        public void Send(IList<IList<string>> tuples, IList<TupleId> tuplesIds, bool isLogging)
        {
            foreach (var tuple in tuples)
            {
                var chosenReplica = Router.Route(tuple);

                Console.WriteLine("[OUT] Sending tuple <{0}> to {1}({2})", string.Join(", ", tuple), Id, chosenReplica);
                try
                {
                    Replicas[chosenReplica].execute(new List<IList<string>> {tuple}, tuplesIds);

                    // TODO: not sure if this is the right place to log. Maybe create a Sender singleton ?
                    if (isLogging)
                    {
                        var stringToLog = "tuple " + ReplicaExecutable.myURL + ", " + string.Join(", ", tuple);
                        ReplicaExecutable.PuppetMaster.log(stringToLog);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Operator] Unable to call execute on another operator. Cause: {0}.", e.Message);
                }
            }
        }
    }
}

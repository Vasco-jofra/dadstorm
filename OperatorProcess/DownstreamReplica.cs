using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using DadStormServices;

namespace ReplicaProcess
{
    public class DownstreamReplica : IDownstreamReplica
    {
        public string OperatorId { get; }
        public int Id { get; }
        public string Url { get; }
        private IOperatorProcess Proxy { get; }

        public DownstreamReplica(string operatorId, int id, string url, IOperatorProcess proxy)
        {
            OperatorId = operatorId;
            Id = id;
            Url = url;
            Proxy = proxy;
        }

        public bool Flow(DadTuple tuple, bool isLogging)
        {
            Console.WriteLine("[OUT] Sending tuple {0} to {1}", tuple, this);

            var sent = false;
            try
            {
                Proxy.Flow(tuple);
                sent = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[OUT] Unable to send tuple to replica {0}. Reason: {1}.", this, e.Message);
            }

            return sent;
        }

        public override string ToString()
        {
            return OperatorId + "(" + Id + ")";
        }

        private static string TupleListToString(IEnumerable<IEnumerable<string>> tuples)
        {
            var temp = tuples.Select(TupleToString);

            return "[" + string.Join(", ", temp) + "]";
        }

        private static string TupleToString(IEnumerable<string> tuple)
        {
            return "<" + string.Join(", ", tuple) + ">";
        }
    }
}

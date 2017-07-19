using System;
using System.Collections.Generic;

namespace ReplicaProcess.Routing
{
    class RandomRoutingStrategy : AbstractRoutingStrategy
    {
        private Random Rnd { get; }

        public RandomRoutingStrategy()
        {
            Rnd = new Random();
        }

        protected override int RoutingAlgorithm(IList<string> tuple, int nReplicas)
        {
            return Rnd.Next(0, nReplicas);
        }
    }
}

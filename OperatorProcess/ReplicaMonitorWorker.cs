using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReplicaProcess
{
    class ReplicaMonitorWorker
    {
        private CancellationTokenSource cts;
        private PeerReplica Replica { get; }

        public ReplicaMonitorWorker(PeerReplica replica)
        {
            cts = new CancellationTokenSource();
            Replica = replica;
        }


        //public async Task StartAsync()
        //{
        //    var dead = false;
        //    while (!dead)
        //    {
        //        await Task.Delay(1000);

        //        try
        //        {
        //            Replica.Ping();
        //        }
        //        catch (Exception)
        //        {
        //            dead = true;
        //        }
        //    }
        //}
    }
}

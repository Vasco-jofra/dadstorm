using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ReplicaProcess
{
    public class MonitoringProcess
    {
        private Timer Timer { get; }
        private IDictionary<IReplica, ICollection<Action<IReplica>>> CallbacksPerReplica { get; }
        private readonly object _elapsedLock = new object();

        public MonitoringProcess()
        {
            Timer = new Timer(3000);
            CallbacksPerReplica = new Dictionary<IReplica, ICollection<Action<IReplica>>>();
            Timer.Elapsed += MonitorAlgorithm;
        }

        public void Start()
        {
            Timer.Start();
        }

        public void Subscribe(IReplica replica, Action<IReplica> callback)
        {
            Console.WriteLine("[MONITOR] Subscribing to death of {0}.", replica);
            lock (this)
            {
                ICollection<Action<IReplica>> replicaCallbacks;
                if (CallbacksPerReplica.TryGetValue(replica, out replicaCallbacks))
                {
                    replicaCallbacks.Add(callback);
                }
                else
                {
                    CallbacksPerReplica.Add(replica, new List<Action<IReplica>> {callback});
                }
            }
        }

        private void MonitorAlgorithm(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Monitor.TryEnter(_elapsedLock))
            {
                try
                {
                    var deadReplicas = new List<IReplica>();

                    foreach (var replicaCallbacks in CallbacksPerReplica)
                    {
                        var replica = replicaCallbacks.Key;
                        var callbacks = replicaCallbacks.Value;

                        Console.WriteLine("[MONITOR] Checking if replica {0} is alive.", replica);

                        try
                        {
                            replica.Ping();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[MONITOR] Noticed replica {0} is dead.", replica);

                            foreach (var callback in callbacks)
                            {
                                callback(replica);
                            }

                            deadReplicas.Add(replica);
                        }
                    }

                    foreach (var deadReplica in deadReplicas)
                    {
                        CallbacksPerReplica.Remove(deadReplica);
                    }
                }
                finally
                {
                    Monitor.Exit(_elapsedLock);
                }
            }
        }
    }
}
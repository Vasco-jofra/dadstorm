using System;
using System.Collections.Generic;
using System.Linq;
using DadStormServices;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.IO;
using ReplicaProcess.Kernel;
using ReplicaProcess.Routing;

namespace ReplicaProcess {
    public class ReplicaExecutable {
        public static readonly string RESOURCES_FOLDER_PATH = @"..\..\..\OperatorProcess\Resources\";
        public static readonly string INPUT_FOLDER_PATH = RESOURCES_FOLDER_PATH + @"input\";


        public static string myURL { get; set; }
        public static IPuppetMaster PuppetMaster { get; set; }

        public static ReplicaServices thisReplica { get; set; }
        
        public static void Main(string[] args) {
            // Check args validity
            if (args.Length != 3) {
                Console.WriteLine("[Operator] Usage: OperatorExecutable <serialized_operator> <replicaID>!");
                Console.WriteLine("[Operator] Received: ");
                foreach (string s in args) {
                    Console.WriteLine(s);
                }
                exiting(5);
                return;
            }

            // Parse operator
            string serializedOperator = args[0];
            Operator operatorData;
            try {
                operatorData = Operator.deserialize(serializedOperator);

            } catch (Exception e) {
                Console.WriteLine("[Operator] Couldn't deserialize the operator. Cause: {0}", e.Message);
                Console.WriteLine("[Operator] Received: {0}", serializedOperator);
                exiting(10);
                return;
            }

            // Parse replicaId
            int replicaID;
            try {
                replicaID = int.Parse(args[1]);
                if (replicaID >= operatorData.replicaURLs.Count)
                    throw new Exception();

            } catch (Exception) {
                Console.WriteLine("[REPLICA] ReplicaID was not a valid or out of range integer!");
                exiting(5);
                return;
            }
            myURL = operatorData.replicaURLs[replicaID];

            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("[REPLICA] Operator: {0}({1})! URL: {2}", operatorData.id, replicaID, myURL);
            Console.WriteLine("------------------------------------------------------------------");


            // initializeOperatorSpec
            var kernel = initializeOperatorSpec(operatorData);
            var downstreamOperators = SetUpDownstream(operatorData);

            // Connect to the puppet master
            string puppetMasterLoggerUrl  = args[2];
            try {
                PuppetMaster = (IPuppetMaster) Activator.GetObject(typeof(IPuppetMaster), puppetMasterLoggerUrl);
                Console.WriteLine("[Replica] Created proxy for puppetMaster at {0}", puppetMasterLoggerUrl);

                PuppetMaster.log("Replica " + operatorData.id + " of operator " + myURL + " launched.");

            } catch (Exception e) {
                Console.WriteLine("[Operator] Unable to talk to the puppetMaster at: {0}. Cause: {1}", puppetMasterLoggerUrl, e.Message);
            }

            //System.Diagnostics.Debugger.Launch();
            var localRoutingStrategy = RoutingPolicyToStrategy(operatorData.routingPolicy, operatorData.HashingField);
            var localReplica = new LocalReplica(operatorData.id, replicaID, null);
            var tupleIdGenerator = new TupleIdGenerator(operatorData.id, replicaID);
            var monitoringProcess = new MonitoringProcess();
            var group = new GroupManager(SetUpGroup(operatorData, replicaID, localReplica), localReplica, monitoringProcess, localRoutingStrategy);
            var tupleProcessor = new TupleProcessor(kernel, downstreamOperators, tupleIdGenerator, group);
            thisReplica = new ReplicaServices(tupleProcessor, group, monitoringProcess, operatorData, operatorData.id, replicaID);
            localReplica.SetProxy(thisReplica);

            // Publish the service
            string replicaURL = operatorData.replicaURLs[replicaID];
            string serviceName = getNameFromURL(replicaURL);

            try {
                int servicePort = getPortFromURL(replicaURL);

                TcpChannel channel = new TcpChannel(servicePort);
                // @SEE: I think because the PCS has already registered a channel and this processe is a child of it we can´t register again?
                //ChannelServices.RegisterChannel(channel, false); // @SEE: should be true??

                Type serviceType = typeof(ReplicaServices);
                RemotingServices.Marshal(thisReplica, serviceName, serviceType);

                System.Console.WriteLine("Started Operator with name {0} at port {1}.", serviceName, servicePort);
            }
            catch (Exception e) {
                Console.WriteLine("[Operator] Unable to publish {0}. Cause: {1}", replicaURL, e.Message);
                exiting(20);
                return;
            }

            // See if we need to read from a file
            foreach(string fileName in operatorData.inputFiles) {
                readTuplesFromFile(INPUT_FOLDER_PATH + fileName, tupleIdGenerator);
            }

            Console.ReadLine();
        }

        private static List<DownstreamOperator> SetUpDownstream(Operator operatorData) {
            var downstreamOperators = new List<DownstreamOperator>();

            // TODO: Clean this into multiple methods to increase readability
            foreach (var outOp in operatorData.Outputs)
            {
                var replicaId = 0;
                var downstreamReplicas = new List<IDownstreamReplica>();

                foreach (var outReplicaUrl in outOp.ReplicasUrl)
                {
                    try
                    {
                        var replicaProxy =
                            (IOperatorProcess) Activator.GetObject(typeof(IOperatorProcess), outReplicaUrl);
                        downstreamReplicas.Add(new DownstreamReplica(outOp.operatorId, replicaId, outReplicaUrl,
                            replicaProxy));
                        Console.WriteLine("[Replica] Created proxy for replica at {0} of the output operator {1}",
                            outReplicaUrl, outOp.operatorId);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            "[Replica] Unable to create a proxy to a replica at {0} of the output operator: {1}. Cause: {2}",
                            outReplicaUrl, outOp.operatorId, e.Message);
                    }
                    finally
                    {
                        replicaId++;
                    }
                }

                var routingStrategy = RoutingPolicyToStrategy(outOp.RoutingPolicy, outOp.HashingField);

                downstreamOperators.Add(new DownstreamOperator(outOp.operatorId, downstreamReplicas, routingStrategy));
            }

            return downstreamOperators;
        }

        private static IList<IReplica> SetUpGroup(Operator operatorData, int ourId, IReplica localReplica)
        {
            var peers = new List<IReplica>();
            var nReplicas = operatorData.replicaURLs.Count;

            for (var peerId = 0; peerId < nReplicas; peerId++)
            {
                if (peerId == ourId)
                {
                    peers.Add(localReplica);
                    continue;
                }

                var peerUrl = operatorData.replicaURLs[peerId];

                try
                {
                    var peerProxy = (IOperatorProcess) Activator.GetObject(typeof(IOperatorProcess), peerUrl);

                    peers.Add(new PeerReplica(operatorData.id, peerId, peerProxy));

                    Console.WriteLine("[Replica] Created proxy to peer at {0}.", peerUrl);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Replica] Unable to create a proxy to peer at {0}. Cause: {1}", peerUrl, e.Message);
                }
            }

            return peers;
        }

        private static ITupleRoutingStrategy RoutingPolicyToStrategy(RoutingPolicy routingPolicy, int hashingField)
        {
            switch (routingPolicy)
            {
                case RoutingPolicy.Primary:
                    return new PrimaryRoutingStrategy();
                case RoutingPolicy.Random:
                    return new RandomRoutingStrategy();
                case RoutingPolicy.Hashing:
                    return new HashingRoutingStrategy(hashingField);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /* Parse given input file into tupleBuffer */
        private static void readTuplesFromFile(string filepath, TupleIdGenerator tupleIdGenerator) {
            TextReader freader;
            string line;

            try {
                freader = File.OpenText(filepath);

            } catch (Exception e) {
                Console.WriteLine("[ERROR] Unable to Open the file: \"{0}\". Proceding without it. {1}", filepath, e.Message);
                return;
            }

            Console.WriteLine("[Operator] Reading from file: {0}", filepath);
            while ((line = freader.ReadLine()) != null) {
                if (line.StartsWith("%")) // ignore comments
                    continue;

                var tuple = line.Split(',').ToList();
                for (int i = 0; i < tuple.Count; i++)
                    tuple[i] = tuple[i].Trim(' '); // strip whitespaces

                thisReplica.Flow(new DadTuple(tupleIdGenerator.NextTupleId(), tuple));
            }
        }

        private static AbstractKernel initializeOperatorSpec(Operator operatorData)
        {
            AbstractKernel kernel;
            switch (operatorData.operatorSpec) {
                case "UNIQ":
                    if (operatorData.operatorSpecArgs.Count == 1) {
                        int fieldN = Int32.Parse(operatorData.operatorSpecArgs[0]);
                        kernel = new UniqKernel(fieldN);
                        break;
                    }
                    throw new Exception("Given Operator Spec arguments are invalid");
                case "DUP":
                    if (operatorData.operatorSpecArgs.Count == 0) {
                        kernel = new DupKernel();
                        break;
                    }
                    throw new Exception("Given Operator Spec arguments are invalid");
                case "COUNT":
                    if (operatorData.operatorSpecArgs.Count == 0) {
                        kernel = new CountKernel();
                        break;
                    }
                    throw new Exception("Given operator spec arguments are invalid");
                case "FILTER":
                    if (operatorData.operatorSpecArgs.Count == 3) {
                        int fieldN = Int32.Parse(operatorData.operatorSpecArgs[0]);
                        char cond = char.Parse(operatorData.operatorSpecArgs[1]);
                        string val = operatorData.operatorSpecArgs[2];
                        kernel = new FilterKernel(fieldN, cond, val);
                        break;
                    }
                    throw new Exception("Given Operator Spec arguments are invalid");
                case "CUSTOM":
                    if (operatorData.operatorSpecArgs.Count == 3) {
                        string dll = operatorData.operatorSpecArgs[0];
                        string cls = operatorData.operatorSpecArgs[1];
                        string method = operatorData.operatorSpecArgs[2];
                        kernel = new CustomKernel(dll, cls, method);
                        break;
                    }
                    throw new Exception("Given Operator Spec arguments are invalid");
                case "OUTPUT":
                    if (operatorData.operatorSpecArgs.Count == 0)
                    {
                        kernel = new OutputKernel();
                        break;
                    }
                    throw new Exception("Given Operator Spec arguments are invalid");
                default:
                    throw new Exception("Given Operator Spec \"" + operatorData.operatorSpec + "\" is invalid");
            }

            return kernel;
        }

        public static void exiting(int seconds) {
            Console.WriteLine("[Operator] Exiting in {0} seconds!", seconds);
            System.Threading.Thread.Sleep(seconds * 1000);
            Environment.Exit(-1);
        }

        private static int getPortFromURL(string url) {
            Uri uri = new Uri(url);
            return uri.Port;
        }

        private static string getNameFromURL(string url) {
            Uri uri = new Uri(url);
            string s = uri.AbsolutePath;
            return uri.AbsolutePath.Substring(1);
        }
    }
}

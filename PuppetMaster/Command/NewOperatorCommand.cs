using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class NewOperatorCommand : PuppetCommand {
        public NewOperatorCommand(PuppetShell shell) : base(shell, "Operator", "Complicated...") { }

        public override void execute(string[] args) {
            /*
            %% The following lines define an operator.
            %% OPERATOR_ID input ops SOURCE_OP_ID1 | FILEPATH1,. . ., SOURCE_OP_IDn | FILEPATHn
            %% rep fact REPL_FACTOR routing primary| hashing | random
            %% address URL1,. . .,URLn
            %% operator spec OPERATOR_TYPE OPERATOR_PARAM1,. . ., OPERATOR_PARAMn
            %%
            */
            string id;
            List<string> inputs = new List<string>();
            int replicationFactor;
            RoutingPolicy routingPolicy;
            int hashingField = 0;
            List<string> replicaURLs = new List<string>();
            string operatorSpec;
            List<string> operatorSpecArgs = new List<string>();

            try {
                Exception e = new Exception();

                id = args[0];
                if (args[1] != "input" || args[2] != "ops") throw e;
                if (args[3] == "rep") throw e; // Must have some inputs first

                int i = 3;
                while (args[i] != "rep") {
                    inputs.Add(args[i]);
                    i++;
                }
                i++;

                if (args[i++] != "fact") throw e;
                replicationFactor = int.Parse(args[i++]);
                if (args[i++] != "routing") throw e;

                string routing = args[i++];
                switch (routing)
                {
                    case "primary":
                        routingPolicy = RoutingPolicy.Primary;
                        break;
                    case "random":
                        routingPolicy = RoutingPolicy.Random;
                        break;
                    case "hashing":
                        routingPolicy = RoutingPolicy.Hashing;
                        hashingField = int.Parse(args[i++]);
                        break;
                    default:
                        throw e;
                }

                if (args[i++] != "address") throw e;

                if (args[i] == "operator") throw e;
                while (args[i] != "operator") {
                    replicaURLs.Add(args[i]);
                    i++;
                }
                i++;
                if (args[i++] != "spec") throw e;
                operatorSpec = args[i];
                i++;

                while (i < args.Length) {
                    operatorSpecArgs.Add(args[i]);
                    i++;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                printMissUsage(args);
                return;
            }

            Operator newOperator = new Operator(id, inputs, replicationFactor, routingPolicy, hashingField, replicaURLs, operatorSpec, operatorSpecArgs);
            shell.operators.Add(id, newOperator);

            try {
                List<IOperatorProcess> opProxies = new List<IOperatorProcess>();
                foreach (string replicaURL in newOperator.replicaURLs) {
                    IOperatorProcess remoteOperator = (IOperatorProcess)Activator.GetObject(typeof(IOperatorProcess), replicaURL);
                    opProxies.Add(remoteOperator);
                    Console.WriteLine("[PuppetMaster] Created proxy for operator at {0}", replicaURL);
                }
                shell.operatorProxies.Add(id, opProxies);
            }
            catch(Exception e) {
                Console.WriteLine("[PuppetMaster] Unable to create a proxy to a replica of the operator: {0}. Cause: {1}", newOperator.id, e.Message);
                return;
            }
        }
    }
}

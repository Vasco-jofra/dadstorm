using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class FreezeCommand : PuppetCommand {
        public FreezeCommand(PuppetShell shell) : base(shell, "Freeze", "Freeze OPERATOR_ID REP_ID: after receiving Freeze, the proecess continues receiving messages but stops processing them.", true) { }

        public override void execute(string[] args) {
            if (args.Length != 2) {
                printMissUsage(args);
                return;
            }

            string operatorId = args[0];
            List<IOperatorProcess> opProxies;
            bool isOperatorIdValid = shell.operatorProxies.TryGetValue(operatorId, out opProxies);
            if (!isOperatorIdValid) {
                printMissUsage(args);
                return;
            }

            int replicaID;
            try {
                replicaID = int.Parse(args[1]);

                // Ensure replicaID in not bigger than the amount of replicas
                if (replicaID >= opProxies.Count)
                    throw new Exception();
            }
            catch (Exception) {
                printMissUsage(args);
                return;
            }

            try {
                opProxies[replicaID].freeze();
            }
            catch (Exception e) {
                Console.WriteLine("[FreezeCommand] Unable to call freeze. Cause: {0}.", e.Message);
            }
        }
    }
}

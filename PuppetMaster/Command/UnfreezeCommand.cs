using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class UnfreezeCommand : PuppetCommand {
        public UnfreezeCommand(PuppetShell shell) : base(shell, "Unfreeze", "Unfreeze OPERATOR_ID REP_ID: a frozen process is back to normal execution. It processes all previous pended messages.", true) { }

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
                opProxies[replicaID].unfreeze();
            }
            catch (Exception e) {
                Console.WriteLine("[UnfreezeCommand] Unable to call unfreeze. Cause: {0}.", e.Message);
            }
        }
    }
}

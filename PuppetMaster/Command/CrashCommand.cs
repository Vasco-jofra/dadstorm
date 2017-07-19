using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class CrashCommand : PuppetCommand {
        public CrashCommand(PuppetShell shell) : base(shell, "Crash", "Crash OPERATOR_ID REP_ID: force a process, i.e. a replica of an operator, to crash.", true) { }

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
                opProxies[replicaID].crash();
            }
            catch (Exception e) {
                Console.WriteLine("[CrashCommand] Unable to call crash. Cause: {0}.", e.Message);
                Console.WriteLine("[CrashCommand] It probably just crashed. ;)");
            }
        }
    }
}

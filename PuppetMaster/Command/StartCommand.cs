using System;
using System.Collections.Generic;
using DadStormServices;

namespace PuppetMaster.Command {
    public class StartCommand : PuppetCommand {
        public StartCommand(PuppetShell shell) : base(shell, "Start", "Start OPERATOR_ID: tells the OPERATOR_ID operator to start processing tuples.", true) { }

        public override void execute(string[] args) {
            if (args.Length != 1) {
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

            foreach (IOperatorProcess opProxy in opProxies) {
                try {
                    opProxy.start();
                }
                catch(Exception e) {
                    Console.WriteLine("[StartCommand] Unable to call start. Cause: {0}.", e.Message);
                }
            }
        }
    }
}

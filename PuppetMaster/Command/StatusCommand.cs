using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class StatusCommand : PuppetCommand {
        public StatusCommand(PuppetShell shell) : base(shell, "Status", "Status: make all nodes in the system to print its current status.", true) { }

        public override void execute(string[] args) {
            if (args.Length != 0) {
                printMissUsage(args);
                return;
            }

            // Send status to every operator
            foreach (KeyValuePair<string, List<IOperatorProcess>> entry in shell.operatorProxies) {
                foreach (IOperatorProcess opProxy in entry.Value) {
                    try {
                        opProxy.status();
                    }
                    catch (Exception e) {
                        Console.WriteLine("[StatusCommand] Unable to call status. Cause: {0}.", e.Message);
                    }
                }
            }
        }
    }
}

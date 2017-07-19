using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class ExitCommand : PuppetCommand {
        public ExitCommand(PuppetShell shell) : base(shell, "Exit", "Exit: Exites the application") { }

        public override void execute(string[] args) {
            if (args.Length != 0) {
                printMissUsage(args);
                return;
            }

            // Kill all programs
            foreach (KeyValuePair<string, List<IOperatorProcess>> entry in shell.operatorProxies) {
                foreach (IOperatorProcess opProxy in entry.Value) {
                    try {
                        opProxy.crash();
                    }
                    catch (Exception e) {
                        Console.WriteLine("[ExitCommand] Unable to call crash. Cause: {0}.", e.Message);
                    }
                }
            }

            System.Environment.Exit(1);
        }
    }
}

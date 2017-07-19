using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class LoggingLevelCommand : PuppetCommand {
        public LoggingLevelCommand(PuppetShell shell) : base(shell, "LoggingLevel", "LoggingLevel full | light", true) { }

        public override void execute(string[] args) {
            if (args.Length != 1) {
                printMissUsage(args);
                return;
            }

            switch (args[0]) {
                case "full":
                    PuppetLogger.Instance.verbosity = LoggingVerbosity.FULL;
                    break;
                case "light":
                    PuppetLogger.Instance.verbosity = LoggingVerbosity.LIGHT;
                    break;
                default:
                    printMissUsage(args);
                    break;
            }

            // Send logging verbocity to every operator
            foreach (KeyValuePair<string, List<IOperatorProcess>> entry in shell.operatorProxies) {
                foreach (IOperatorProcess opProxy in entry.Value) {
                    try {
                        if (PuppetLogger.Instance.verbosity == LoggingVerbosity.LIGHT) {
                            opProxy.stopLogging();
                        }
                        else {
                            opProxy.startLogging();
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine("[Logging] Unable to call logging. Cause: {0}.", e.Message);
                    }
                }
            }

        }
    }
}

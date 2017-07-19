using DadStormServices;
using System;
using System.Collections.Generic;

namespace PuppetMaster.Command {
    public class IntervalCommand : PuppetCommand {
        public IntervalCommand(PuppetShell shell) : base(shell, "Interval", "Interval OPERATOR_ID X ms: the operator should sleep X milliseconds between consecutive events.", true) { }

        public override void execute(string[] args) {
            string operatorId;
            int miliseconds = 0;
            if (args.Length != 2) {
                printMissUsage(args);
                return;
            }

            try {
                operatorId = args[0];
                miliseconds = int.Parse(args[1]);
            }
            catch (Exception) {
                printMissUsage(args);
                return;
            }

            List<IOperatorProcess> opProxies;
            bool isOperatorIdValid = shell.operatorProxies.TryGetValue(operatorId, out opProxies);
            if (!isOperatorIdValid) {
                printMissUsage(args);
                return;
            }

            foreach (IOperatorProcess opProxy in opProxies) {
                try {
                    opProxy.interval(miliseconds);
                }
                catch (Exception e) {
                    Console.WriteLine("[IntervalCommand] Unable to call interval. Cause: {0}.", e.Message);
                }
            }
        }
    }
}

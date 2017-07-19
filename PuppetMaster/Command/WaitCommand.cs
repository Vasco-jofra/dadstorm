using System;

namespace PuppetMaster.Command {
    public class WaitCommand : PuppetCommand {
        public WaitCommand(PuppetShell shell) : base(shell, "Wait", "Wait <miliseconds>") { }

        public override void execute(string[] args) {
            int miliseconds = 0;
            if (args.Length != 1) {
                printMissUsage(args);
                return;
            }
            try {
                miliseconds = int.Parse(args[0]);
            } catch (Exception) {
                printMissUsage(args);
                return;
            }

            Console.WriteLine("Waiting for {0}ms...", miliseconds);
            System.Threading.Thread.Sleep(miliseconds);
            Console.WriteLine("Waking up now!", miliseconds);
        }
    }
}

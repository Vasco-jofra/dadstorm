using System;

namespace PuppetMaster.Command {
    public class HelpCommand : PuppetCommand {
        public HelpCommand(PuppetShell shell) : base(shell, "Help", "Help: helps") { }

        public override void execute(string[] args) {
            if (args.Length != 0) {
                printMissUsage(args);
                return;
            }

            Console.WriteLine("Available Commands are: ");
            foreach (PuppetCommand c in shell.commands) {
                Console.WriteLine("   " + c.usage);
            }
        }
    }
}

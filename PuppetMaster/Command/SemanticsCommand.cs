namespace PuppetMaster.Command {
    public class SemanticsCommand : PuppetCommand {
        public SemanticsCommand(PuppetShell shell) : base(shell, "Semantics", "Semantics at-most-once | at-least-once | exactly-once") { }

        public override void execute(string[] args) {
            if (args.Length != 1) {
                printMissUsage(args);
                return;
            }

            switch (args[0]) {
                case "at-most-once":
                    shell.semantics = Semantics.AT_MOST_ONCE;
                    break;
                case "at-least-once":
                    shell.semantics = Semantics.AT_LEAST_ONCE;
                    break;
                case "exactly-once":
                    shell.semantics = Semantics.EXACLY_ONCE;
                    break;
                default:
                    printMissUsage(args);
                    break;
            }
        }
    }
}

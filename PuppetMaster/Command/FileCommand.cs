namespace PuppetMaster.Command {
    public class FileCommand : PuppetCommand {
        public FileCommand(PuppetShell shell) : base(shell, "File", "File FILE_PATH: @TODO.") { }

        public override void execute(string[] args) {
            if (args.Length != 1) {
                printMissUsage(args);
                return;
            }

            string filePath = Program.INPUT_FOLDER_PATH + args[0];
            shell.readFile(filePath);
        }
    }
}

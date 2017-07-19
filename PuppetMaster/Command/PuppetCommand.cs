using System;

namespace PuppetMaster.Command {
    abstract public class PuppetCommand {
        public PuppetShell shell { get; }
        public string name { get; protected set; }
        public string usage { get; protected set; }
        public bool isAssynchronous { get; protected set; }

        public PuppetCommand(PuppetShell shell, string name, string usage, bool isAssynchronous = false) {
            this.shell = shell;
            this.name = name;
            this.usage = usage;
            this.isAssynchronous = isAssynchronous;
        }

        abstract public void execute(string[] args);

        public void execute(object args) {
            string[] realArgs;
            try {
                realArgs = (string[])args;
            } catch (Exception) {
                Console.Write("[Command] Args were not an array of strings. (Should probably never be reached)");
                return;
            }
            execute(realArgs);
        }

        public void printMissUsage(string[] args) {
            Console.Write("[Incorrect usage]: \"{0}", name);
            foreach(string s in args) {
                Console.Write(" {0}", s);
            }
            Console.WriteLine("\"");
            Console.WriteLine("->[Correct usage]: " + usage);
        }
    }
}

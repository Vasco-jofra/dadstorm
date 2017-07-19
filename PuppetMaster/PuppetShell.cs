using System;
using System.Collections.Generic;
using System.Linq;
using PuppetMaster.Command;
using System.IO;
using DadStormServices;
using System.Threading;

namespace PuppetMaster {
    public class PuppetShell {
        private string prompt = ">>>";
        private char commentChar = '%';
        public List<PuppetCommand> commands { get; }
        private TextReader stdin { get; set; }
        public bool readingFile { get; set; }

        public Dictionary<string, Operator> operators { get; }
        public Dictionary<string, List<IOperatorProcess>> operatorProxies { get; }
        public Semantics semantics { get; set; }

        public Dictionary<string, IProcessCreator> PCSs { get; }

        public PuppetShell() {
            readingFile = false;

            commands = new List<PuppetCommand>();
            operators = new Dictionary<string, Operator>();
            operatorProxies = new Dictionary<string, List<IOperatorProcess>>();
            PCSs = new Dictionary<string, IProcessCreator>();

            commands.Add(new StartCommand(this));
            commands.Add(new IntervalCommand(this));
            commands.Add(new StatusCommand(this));
            commands.Add(new CrashCommand(this));
            commands.Add(new FreezeCommand(this));
            commands.Add(new UnfreezeCommand(this));
            commands.Add(new WaitCommand(this));

            commands.Add(new LoggingLevelCommand(this));
            commands.Add(new FileCommand(this));

            commands.Add(new HelpCommand(this));
            commands.Add(new ExitCommand(this));

            connectToPCSs();
            parseMainConfigFile();
        }

        public void readFile(string filePath) {
            
            try {
                readingFile = true;
                stdin = Console.In; //Save stdin

                TextReader fileInput = File.OpenText(filePath);
                Console.SetIn(fileInput);
            } catch (Exception) {
                Console.WriteLine("[ERROR] Unable to Open the file: \"{0}\". Proceding as a manual console.", filePath);
            }
        }

        public void resetStdin() {
            Console.WriteLine(" [Return to manual mode]");
            Console.SetIn(stdin);
            readingFile = false;
        }

        public PuppetCommand getCommand(string commandString) {
            foreach (PuppetCommand c in commands) {
                if (string.Equals(c.name, commandString, StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return null;
        }

        public bool runCommand(string inputLine) {
            string[] args = inputLine.Split(' ');
            string commandStr = args[0];

            if (commandStr != "" && commandStr.ElementAt(0) == commentChar) // Jumps spaces and comments (%).
                return false;
            else if (commandStr == "")
                return true;

            PuppetCommand command = getCommand(commandStr);
            if (command == null) {
                Console.WriteLine("Invalid command <{0}>.", commandStr);
            }
            else {
                string[] commandArgs = args.Skip(1).ToArray();

                if (command.isAssynchronous == false) {
                    command.execute(commandArgs); // Removes the first argument aka the command string

                } else {
                    Thread t = new Thread(new ParameterizedThreadStart(command.execute));
                    t.Start(commandArgs);
                }

                PuppetLogger.Instance.log(inputLine);
            }

            return true;
        }

        public void run() {
            bool shouldPrompt = true;
            while (true) {
                if (!readingFile && shouldPrompt)
                    Console.Write(prompt);
                else
                    shouldPrompt = true;

                string inputLine = Console.ReadLine();
                if (inputLine == null) { // The script file reached the end
                    resetStdin();
                    continue;
                }

                shouldPrompt = runCommand(inputLine);
            }
        }


        /********************************************************/
        /**************** CONFIG_FILE_STUFF *********************/
        /********************************************************/
        private void connectToPCSs() {
            TextReader fileInput;
            string PCSConfigFilePath = Program.PCS_CONFIG_FILE;

            try {
                fileInput = File.OpenText(PCSConfigFilePath);
            } catch (Exception) {
                Console.WriteLine("[PCS] Unable to open PCS_Config_File at: {0}.", PCSConfigFilePath);
                return;
            }

            string inputLine;
            while ((inputLine = fileInput.ReadLine()) != null) {
                string PCSUrl = inputLine; // just because
                IProcessCreator pcs;
                try {
                    pcs = (IProcessCreator) Activator.GetObject(typeof(IProcessCreator), PCSUrl);
                    PCSs.Add(getIPFromURL(PCSUrl), pcs);
                    Console.WriteLine("[PCS] Created proxy for PCS at {0}", PCSUrl);
                } catch (Exception e) {
                    Console.WriteLine("[PCS] Unable to create proxy for PCS at: {0}. Cause: {1}", PCSUrl, e.Message);
                }
            }
        }

        private void parseMainConfigFile() {
            // Vars
            TextReader fileInput;
            string mainConfigFilePath;
            bool shouldStep = false;

            // Input for config filename
            Console.WriteLine("[CONFIG] Specify the config file (<enter> for default path \"{0}\")", Program.MAIN_CONFIG_NAME);
            Console.Write("[CONFIG] FileName: ");
            string inputConfigFile = Console.ReadLine();

            if(inputConfigFile == "") {
                mainConfigFilePath = Program.CONFIG_FOLDER_PATH + Program.MAIN_CONFIG_NAME;
            } else {
                mainConfigFilePath = Program.CONFIG_FOLDER_PATH + inputConfigFile;
            }

            try {
                fileInput = File.OpenText(mainConfigFilePath);
                Console.WriteLine("[CONFIG] Open config file at: {0}.", mainConfigFilePath);

            }
            catch (Exception) {
                Console.WriteLine("[CONFIG] Unable to open MAIN_CONFIG_FILE at: {0}.", mainConfigFilePath);
                return;
            }

            // Input for step or auto
            Console.WriteLine("[CONFIG] Specify whether you want to step or autorun.");
            Console.Write("[CONFIG] Step?(y/n): ");
            string shouldStepStr = Console.ReadLine();

            if(shouldStepStr == "y") {
                shouldStep = true;
            }

            // Add commands
            PuppetCommand c1 = new SemanticsCommand(this);
            PuppetCommand c2 = new NewOperatorCommand(this);
            commands.Add(c1);
            commands.Add(c2);


            // Parse file
            bool startedReadingOperators = false;
            bool finishedReadingOperators = false;
            string inputLine;

            while ((inputLine = fileInput.ReadLine()) != null) {
                inputLine = inputLine.TrimEnd(' ');
                string[] args = inputLine.Split(' ');
                string commandStr = args[0];

                // Have to do this here as well unfortunetly, or else operator fails to be correctly parsed
                if (commandStr == "" || commandStr.ElementAt(0) == commentChar)
                    continue;

                // Step
                if (shouldStep == true) {
                    Console.Write("INPUT: \"{0}\". (<enter> to step.)", inputLine);
                    Console.ReadLine();
                }

                // If it is an unknown command assume it is an operator
                if (getCommand(commandStr) == null) {
                    startedReadingOperators = true;
                    inputLine = string.Concat("Operator ", inputLine);
                    inputLine = inputLine.Replace(',', ' ');
                    inputLine = inputLine.Replace("  ", " ");
                }
                else if (startedReadingOperators == true && finishedReadingOperators == false) {
                    finishedReadingOperators = true;
                    startOperatorProcesses();
                    
                    // Wait a bit to let the operators start executing and get published
                    // @SEE: Maybe this wont be good enought for slow systems
                    Thread.Sleep(2000);
                }

                runCommand(inputLine);
            }

            // If operators was the last thing we read, we need to start the operators
            if (startedReadingOperators == true && finishedReadingOperators == false) {
                startOperatorProcesses();
            }

            // remove commands
            commands.Remove(c1);
            commands.Remove(c2);
            Console.WriteLine("[CONFIG] Sucessfully read main config file!");
        }


        private void setOperatorOutputs() {
            foreach (KeyValuePair<string, Operator> entry in operators) {
                string operatorId = entry.Key;
                Operator outputOp = entry.Value;

                foreach(string input in outputOp.inputs) {
                    Operator inputOp;

                    bool isOperator = operators.TryGetValue(input, out inputOp);
                    if (isOperator) {
                        inputOp.Outputs.Add(new OutputOperatorDTO(operatorId, outputOp.replicaURLs, outputOp.routingPolicy, outputOp.HashingField));
                    }
                    else { // Is file
                        outputOp.inputFiles.Add(input);
                    }
                }
            }
        }

        private void startOperatorProcesses() {
            setOperatorOutputs();

            string serializedOperator;

            foreach (KeyValuePair<string, Operator> entry in operators) {
                // For each replica of the operator, tell a PCS (the one that belongs to the target machine) to start the process.
                Operator o = entry.Value;
                serializedOperator = o.serialize();

                for (int i = 0; i < o.replicaURLs.Count; i++) {
                    IProcessCreator pcs;
                    string replicaURL = o.replicaURLs[i];
                    string replicaIP = getIPFromURL(replicaURL);

                    if (PCSs.TryGetValue(replicaIP, out pcs) == false) {
                        Console.WriteLine("[PCS] PCS in machine {0} (ip = {1}) does not exist. Unable to launch OperatorProcess.", replicaURL, replicaIP);
                        return;
                    }

                    try {
                        // @FIXME
                        // IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName()).Where(ip => !IPAddress.IsLoopback(ip)).ToArray();
                        // pcs.createProcess(@"tcp://" + localIPs[0].ToString() + ":" + Program.PUPPET_MASTER_PORT + "/" + Program.PUPPET_MASTER_LOGGER_NAME, serializedOperator, i);
                        pcs.createProcess(@"tcp://localhost:" + Program.PUPPET_MASTER_PORT + "/" + Program.PUPPET_MASTER_LOGGER_NAME, serializedOperator, i);
                        Console.WriteLine("[PCS] Asked pcs to run this replica - {0}.", replicaURL);

                    } catch (Exception e) {
                        Console.WriteLine("{0}", e);
                        Console.WriteLine("[PCS] Unable to reach PCS at: {0}. Cause: {1}. (Removing..)", pcs, e.Message);
                        PCSs.Remove(replicaIP);
                    }
                }
            }

            // Send if it should log or not
            if (PuppetLogger.Instance.verbosity == LoggingVerbosity.LIGHT) {
                runCommand("LoggingLevel light");
            }
            else {
                runCommand("LoggingLevel full");
            }
        }

        // Parse something like this "tcp://1.2.3.5:11000/op" and return "1.2.3.5"
        public string getIPFromURL(string url) {
            Uri uri = new Uri(url);
            return uri.Host;
        }
    }


    public enum Semantics { AT_MOST_ONCE, AT_LEAST_ONCE, EXACLY_ONCE }
}

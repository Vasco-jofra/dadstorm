using System;
using System.Diagnostics;
using DadStormServices;

namespace ProcessCreationService {
    public class ProcessCreationService : MarshalByRefObject, IProcessCreator {
        private const string OPERATOR_EXECUTABLE_PATH = @"..\..\..\OperatorProcess\bin\Debug\OperatorProcess.exe";

        public void createProcess(string puppetUrl, string serializedOperator, int replicaId) {
            string argument = serializedOperator;
            argument = argument.Replace("\"", "\"\"");
            argument = argument.Replace("\\", "\\\\");

            Process myProcess = new Process();
            try {
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = OPERATOR_EXECUTABLE_PATH;
                myProcess.StartInfo.CreateNoWindow = false;

                myProcess.StartInfo.Arguments = "\"" + argument + "\"" + " " + replicaId + " " + puppetUrl;
                myProcess.Start();
                // Console.WriteLine("Sucessfully launched a new operator with args: {0}!", argument);
            }
            catch (Exception e) {
                Console.WriteLine("[PCS] Unable to create a process for the new operator. Caused by: {0}." + e.Message);
            }
        }
    }
}

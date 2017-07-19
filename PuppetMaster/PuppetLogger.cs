using System;
using System.IO;

namespace PuppetMaster {
    public enum LoggingVerbosity {LIGHT, FULL}


    public class PuppetLogger {
        private static readonly object padlock = new object();
        private static PuppetLogger instance;
        private StreamWriter logFile;
        public LoggingVerbosity verbosity { get; set; }

        private PuppetLogger() {
            verbosity = LoggingVerbosity.LIGHT;
            string filePath = getLogFileName();
            try {
                Directory.CreateDirectory(Program.LOG_FOLDER_PATH);
                logFile = new StreamWriter(filePath, true);
            } catch(Exception e) {
                Console.WriteLine("[ERROR] PuppetLogger unable to open/create the log file: " + filePath);
                Console.WriteLine("   " + e.Message);
            }
        }

        private string getLogFileName() {
            DateTime thisDay = DateTime.Now;
            string dateString = thisDay.ToString("dd-MM-yyyy_HH-mm-ss");

            return Program.LOG_FOLDER_PATH + "log_" + dateString + ".log";
        }
        public static PuppetLogger Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new PuppetLogger();
                    }
                    return instance;
                }
            }
        }

        public void log(string stringToLog) {
            lock(this) {  // @SEE: needed?
                if (logFile != null) {
                    logFile.WriteLine(stringToLog);
                    logFile.Flush();
                }
            }
        }
    }
}

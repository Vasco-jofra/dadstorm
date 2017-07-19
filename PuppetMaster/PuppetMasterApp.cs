using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PuppetMaster {
    class Program {
        // Keeping all files here to have a central place to get them. Can be done with config files as well.
        public static readonly string RESOURCES_FOLDER_PATH = @"..\..\Resources\";

        public static readonly string INPUT_FOLDER_PATH = RESOURCES_FOLDER_PATH + @"Inputs\";
        public static readonly string LOG_FOLDER_PATH = RESOURCES_FOLDER_PATH + @"Logs\";
        public static readonly string CONFIG_FOLDER_PATH = RESOURCES_FOLDER_PATH + @"Config\";

        public static readonly string PCS_CONFIG_FILE = CONFIG_FOLDER_PATH + "PCSUrls.conf";
        public static readonly string MAIN_CONFIG_NAME = @"dadstorm.config.full_nounder";


        public static readonly string PUPPET_MASTER_LOGGER_NAME = "PuppetMasterLogger";

        public static readonly int PUPPET_MASTER_PORT = 10001;


        static void Main(string[] args) {
            // Open port for logging purposes
            TcpChannel channel = new TcpChannel(PUPPET_MASTER_PORT);
            ChannelServices.RegisterChannel(channel, false); // @SEE: should be true??
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PuppetMasterService),
                PUPPET_MASTER_LOGGER_NAME,
                WellKnownObjectMode.Singleton);


            // Create shell, and read from file at args[0] if it exists
            PuppetShell sh = new PuppetShell();

            bool hasInputFile = args.Length > 0;
            if (hasInputFile) {
                string filePath = args[0];
                sh.runCommand("file " + filePath);
            }

            sh.run();
        }
    }
}

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace ProcessCreationService {
    class ProcessCreationApp {
        private const int PCS_PORT = 10000;
        public const string PCS_NAME = "PCS";

        static void Main(string[] args) {
            var channel = new TcpChannel(PCS_PORT);
            ChannelServices.RegisterChannel(channel, false); // @SEE: should be true??
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ProcessCreationService),
                PCS_NAME,
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("Started PCS with name {0} at port {1}.", PCS_NAME, PCS_PORT);
            System.Console.WriteLine("(Press any key to kill server.)");
            System.Console.ReadLine();
        }
    }
}

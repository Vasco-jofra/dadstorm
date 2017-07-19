using System;
using DadStormServices;

namespace PuppetMaster {
    class PuppetMasterService : MarshalByRefObject, IPuppetMaster {
        public void log(string stringToLog) {
            PuppetLogger.Instance.log(stringToLog);
        }
    }
}
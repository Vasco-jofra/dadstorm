using System;
using System.Collections.Generic;

namespace ReplicaProcess.Kernel {
    public class DupKernel : AbstractKernel {
        public override IList<IList<string>> execute(IList<string> tuple) {
            if (DEBUGGING_HARD) Console.WriteLine("[DupKernel] Got tuple <{0}>", string.Join(", ", tuple));

            return new List<IList<string>> {tuple};
        }
    }
}

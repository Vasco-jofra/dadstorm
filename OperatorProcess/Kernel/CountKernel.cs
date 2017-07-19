using System;
using System.Collections.Generic;

namespace ReplicaProcess.Kernel {
    public class CountKernel : AbstractKernel {
        private int counter {get; set;}
        public CountKernel(){ this.counter = 0; }
        public override IList<IList<string>> execute(IList<string> list) {
            this.counter++;
            if(DEBUGGING_HARD) Console.WriteLine("[CountKernel] Counter {0}", this.counter.ToString());

            // create output
            List<string> tup = new List<string>();
            tup.Add(counter.ToString());
            List<IList<string>> nested = new List<IList<string>>();
            nested.Add(tup);
            return nested;
        }
    }
}

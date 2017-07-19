using System;
using System.Collections.Generic;
using DadStormServices;

namespace ReplicaProcess.Kernel {
    public class UniqKernel : AbstractKernel {
        private int fieldNum { get; }
        private List<string> history { get; }

        public UniqKernel(int fieldNum) {
            this.fieldNum = fieldNum - 1; // because field number is 1 based
            this.history = new List<string>();
        }

        public override IList<IList<string>> execute(IList<string> tuple) {
            var outputTuples = new List<IList<string>>();

            if (this.fieldNum < tuple.Count) {
                if (!history.Contains(tuple[fieldNum])) {
                    if (DEBUGGING_HARD) Console.WriteLine("[UniqKernel] Field is unique: {0}", tuple[fieldNum]);
                    history.Add(tuple[fieldNum]);
                    outputTuples.Add(tuple);
                }
            }
            else {
                if (DEBUGGING_HARD) Console.WriteLine("[FilterKernel] Field number inacessible in tuple");
            }

            return outputTuples;
        }

        public override void TupleWasProcessed(DadTuple upstreamTupleThatWasProcessed)
        {
            history.Add(upstreamTupleThatWasProcessed.Content[fieldNum]);
        }
    }
}

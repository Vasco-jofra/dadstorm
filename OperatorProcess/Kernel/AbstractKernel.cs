using System.Collections.Generic;
using DadStormServices;

namespace ReplicaProcess.Kernel {
    public abstract class AbstractKernel {
        public static readonly bool DEBUGGING_HARD = false;

        public abstract IList<IList<string>> execute(IList<string> tuple);
        public virtual void TupleWasProcessed(DadTuple upstreamTupleThatWasProcessed) { }
    }
}

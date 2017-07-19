using System.Collections.Generic;

namespace ReplicaProcess.Kernel {
    public class OutputKernel : AbstractKernel {
        public override IList<IList<string>> execute(IList<string> tuple) {
            const string outputFile = "output.txt";

            using (var file = new System.IO.StreamWriter(outputFile, true))
            {
                foreach (var line in tuple)
                {
                    file.WriteLine(line);
                }
            }

            IList<IList<string>> result = new List<IList<string>>();
            result.Add(tuple);
            return result;
        }
    }
}

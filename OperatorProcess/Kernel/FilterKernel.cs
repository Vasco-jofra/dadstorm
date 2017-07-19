using System;
using System.Collections.Generic;

namespace ReplicaProcess.Kernel {
    public class FilterKernel : AbstractKernel {
        private int fieldNum { get; }
        private char cond { get; }
        private string val { get; }
        public FilterKernel(int fieldNum, char cond, string val) {
            this.fieldNum = fieldNum - 1; // because field number is 1 based
            this.cond = cond;
            this.val = val;
        }

        public override IList<IList<string>> execute(IList<string> tuple) {
            List<IList<string>> nested = new List<IList<string>>();
            if (this.fieldNum < tuple.Count) {
                if (cond == '<') {
                    try {
                        int v = Int32.Parse(this.val);
                        int f = Int32.Parse(tuple[fieldNum]);
                        if (f < v) nested.Add(tuple); // return tuple
                    }
                    catch (System.FormatException) { } // ignore, will return empty tuple list
                }
                else if (cond == '=') {
                    try {
                        int v = Int32.Parse(this.val);
                        int f = Int32.Parse(tuple[fieldNum]);
                        if (f == v) nested.Add(tuple); // return tuple
                    }
                    catch (System.FormatException) { // compare as strings
                        if (tuple[fieldNum] == this.val) nested.Add(tuple); // reeturn tuple
                    }
                }
                else if (cond == '>') {
                    try {
                        int v = Int32.Parse(this.val);
                        int f = Int32.Parse(tuple[fieldNum]);
                        if (f > v) nested.Add(tuple); // return tuple
                    }
                    catch (System.FormatException) { } // ignore, will return empty tuple list
                }
                if (DEBUGGING_HARD) Console.WriteLine(nested.Count == 0 ? "[FilterKernel] False condition: {0} {1} {2}" : "[FilterKernel] True Condition: {0} {1} {2}", tuple[fieldNum], this.cond, this.val);
            }
            else {
                if (DEBUGGING_HARD) Console.WriteLine("[FilterKernel] Field number inacessible in tuple");
            }
            return nested;
        }
    }
}

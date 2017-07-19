using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ReplicaProcess.Kernel {
    public class CustomKernel : AbstractKernel {
        private string methodName { get; }
        private Type type { get; }

        public CustomKernel(string dllPath, string className, string methodName) {
            this.methodName = methodName;

            try {
                var code = File.ReadAllBytes(dllPath);
                var assembly = Assembly.Load(code);

                this.type = assembly.GetTypes().First(type => type.IsClass && type.FullName.EndsWith("." + className));

            } catch(Exception e) {
                Console.WriteLine("Unable to load dll at {0}. Cause: {1}", dllPath, e.Message);
                ReplicaExecutable.exiting(5);
            }
        }

        public override IList<IList<string>> execute(IList<string> tuple) {
            var classObj = Activator.CreateInstance(type);

            // Dynamically Invoke the method
            object[] args = { tuple };
            var resultObject = type.InvokeMember(
                this.methodName,
                BindingFlags.Default | BindingFlags.InvokeMethod,
                null,
                classObj,
                args);

            var returnedTuples = (IList<IList<string>>) resultObject;

            if (DEBUGGING_HARD) Console.WriteLine("[CustomKernel] " + string.Join(", ", tuple) + " -> " + tupleCollectionToString(returnedTuples));

            return returnedTuples;
        }

        private static string tupleCollectionToString(IList<IList<string>> tuples)
        {
            return "[" + string.Join("; ", tuples.Select(tuple => "<" + string.Join(", ", tuple) + ">")) + "]";
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DadStormServices;
using ReplicaProcess.Kernel;

namespace ReplicaProcess
{
    public class TupleProcessor
    {
        private static readonly string OutputFolderPath = ReplicaExecutable.RESOURCES_FOLDER_PATH + @"output\";

        private BufferBlock<DadTuple> TupleBuffer { get; }
        private bool IsFrozen { get; set; }
        private bool IsLogging { get; set; }
        private AbstractKernel Kernel { get; }
        private List<DownstreamOperator> DownstreamOperators { get; }
        private TupleIdGenerator TupleIdGenerator { get; }
        private GroupManager Group { get; }
        private string outputFilePath;

        public TupleProcessor(AbstractKernel kernel, List<DownstreamOperator> downstreamOperators, TupleIdGenerator tupleIdGenerator, GroupManager group)
        {
            TupleBuffer = new BufferBlock<DadTuple>();
            Kernel = kernel;
            DownstreamOperators = downstreamOperators;
            TupleIdGenerator = tupleIdGenerator;
            Group = group;

            outputFilePath = GetOutputFileName();
        }

        // Consumes the tuples in tupleBuffer
        private async Task consumer()
        {
            Console.WriteLine("[CONSUMER] Starting tuple consumer - {0}", Kernel.ToString().Split('.').Last());

            while (await TupleBuffer.OutputAvailableAsync())
            {
                SpinWait.SpinUntil(() => !IsFrozen); // Wait for operator to be unfrozen

                DadTuple inputTuple;
                while (TupleBuffer.TryReceive(out inputTuple))
                {
                    Process(inputTuple);
                }
            }

            Console.WriteLine("Leaving tuple consumer...");
        }

        private void Process(DadTuple inputTuple)
        {
            Console.WriteLine("[CONSUMER] Starting computation of {0}.", inputTuple);
            
            // Process the tuple
            var computedTuples = Kernel.execute(inputTuple.Content);
            if (computedTuples.Count == 0)
                return;

            var outTuples = computedTuples.Select(t => new DadTuple(TupleIdGenerator.NextTupleId(), t)).ToList();

            Console.WriteLine("[CONSUMER] Sharing results of computation of {0} to group.", inputTuple);
            Group.RMSend(p => p.SaveProcessedTuples(inputTuple.Id, outTuples));
            //Group.LocalReplica.SaveProcessedTuples(inputTuple.Id, outTuples);


            // Is the last operator in the acyclic graph
            if (DownstreamOperators.Count == 0) 
            {
                // Output to a file (if we are the leader)
                OutputToFile(outTuples);
            }
            else 
            {
                // Send the tuples
                SendOutputTuples(outTuples);
            }

            // Log
            LogToPuppetMaster(outTuples);

            Group.RMSend(p => p.DeliveredTuples(inputTuple.Id, outTuples.Select(ot => ot.Id).ToList()));
        }


        public bool Post(DadTuple tuple)
        {
            return TupleBuffer.Post(tuple);
        }

        public async Task<bool> SendAsync(DadTuple tuple)
        {
            return await TupleBuffer.SendAsync(tuple);
        }

        private void OutputToFile(IList<DadTuple> outTuples) {
            //if(Group.AmILeader()) {
                using (TextWriter file = new StreamWriter(outputFilePath, true))
                {
                    foreach (DadTuple tuple in outTuples)
                    {
                        if (tuple.Content.Count == 0) continue;

                        Console.WriteLine("[OUT][WRITING_TO_FILE] " + string.Join(", ", tuple.Content));
                        file.WriteLine(string.Join(", ", tuple.Content));
                    }
                }
            //}
        }

        private void LogToPuppetMaster(IList<DadTuple> outTuples) 
        {
            if (IsLogging) 
            {
                foreach (DadTuple tuple in outTuples) {
                    var stringToLog = "tuple " + ReplicaExecutable.thisReplica.OperatorID + ", " + ReplicaExecutable.myURL + ", " + string.Join(", ", tuple.Content);
                    Console.WriteLine("[OUT][LOGGING] " + stringToLog);
                    ReplicaExecutable.PuppetMaster.log(stringToLog);
                }
           }
        }


        private void SendOutputTuples(IList<DadTuple> tuples)
        {
            foreach (var outputOperator in DownstreamOperators)
            {
                outputOperator.Flow(tuples, IsLogging);
            }
        }

        private static string GetOutputFileName()
        {
            DateTime thisDay = DateTime.Now;
            string dateString = thisDay.ToString("dd-MM-yyyy_HH-mm-ss");

            return OutputFolderPath + "out_" + dateString + ".txt";
        }

        public void Freeze()
        {
            IsFrozen = true;
        }

        public void Unfreeze()
        {
            IsFrozen = false;
        }

        public void StartLogging()
        {
            IsLogging = true;
        }

        public void StopLogging()
        {
            IsLogging = false;
        }

        public void Start()
        {
            Task.Run(consumer);
        }

        public void TupleWasProcessed(DadTuple upstreamTupleThatWasProcessed)
        {
            Kernel.TupleWasProcessed(upstreamTupleThatWasProcessed);
        }
    }
}

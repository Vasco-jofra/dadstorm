using System.Collections.Generic;

namespace DadStormServices {
    /* Remote interfaces */
    public interface IProcessCreator {
        void createProcess(string puppetUrl, string serializedOperator, int replicaId);
    }

    public interface IPuppetMaster {
        void log(string dataToLog);
    }

    public interface IOperatorProcess {
        void start();
        void interval(int milisseconds);
        void status();
        void crash();
        void freeze();
        void unfreeze();
        void startLogging();
        void stopLogging();
        void Ping();

        void Flow(DadTuple tuple);
        void Share(DadTuple tuple, int senderId);
        void SetOwner(DadTuple tuple, int ownerId);
        void SaveProcessedTuples(DadTupleId oldId, List<DadTuple> outTuples);
        void DeliveredTuples(DadTupleId inputId, List<DadTupleId> outputIds);
    }
}

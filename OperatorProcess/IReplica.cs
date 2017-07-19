using System.Collections.Generic;
using DadStormServices;

namespace ReplicaProcess
{
    public interface IReplica
    {
        int ReplicaId { get; }
        void AddOwnedTuple(DadTuple tuple);
        IEnumerable<DadTuple> PopOwnedTuples();
        void SetOwner(DadTuple tuple, int ownerId);
        void Share(DadTuple tuple, int replicaId);
        void SaveProcessedTuples(DadTupleId oldId, List<DadTuple> processedTuples);
        void DeliveredTuples(DadTupleId inputId, List<DadTupleId> outputIds);
        void Ping();
    }
}
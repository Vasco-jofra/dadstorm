using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadStormServices
{
    [Serializable]
    public class DadTupleId
    {
        public string OperatorId { get; }
        public int ReplicaId { get; }
        public long SeqNumber { get; }

        public DadTupleId(string operatorId, int replicaId, long seqNumber)
        {
            OperatorId = operatorId;
            ReplicaId = replicaId;
            SeqNumber = seqNumber;
        }

        public override string ToString()
        {
            return OperatorId + "_" + ReplicaId + "_" + SeqNumber;
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to DadTupleId return false.
            DadTupleId other = obj as DadTupleId;
            if ((object)other == null)
            {
                return false;
            }

            // Return true if the fields match:
            return string.Equals(OperatorId, other.OperatorId) && (ReplicaId == other.ReplicaId) && (SeqNumber == other.SeqNumber);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (OperatorId != null ? OperatorId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ ReplicaId;
                hashCode = (hashCode*397) ^ SeqNumber.GetHashCode();
                return hashCode;
            }
        }
    }
}

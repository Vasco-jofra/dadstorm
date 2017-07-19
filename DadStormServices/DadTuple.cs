using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DadStormServices
{
    [Serializable]
    public class DadTuple
    {
        public DadTupleId Id { get; }
        public IList<string> Content { get; }
        
        public DadTuple(DadTupleId id, IList<string> content)
        {
            Id = id;
            Content = content;
        }

        public override string ToString()
        {
            return "{id:" + Id + " content:<" + string.Join(", ", Content) +">}";
        }
    }
}

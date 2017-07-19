using System.Collections.Generic;
using System.Linq;

namespace LibTestCustomOperations
{
    public class Examples
    {
        public IList<IList<string>> UpperCase(List<string> tuple)
        {
            return new List<IList<string>>
            {
                tuple.Select(s => s.ToUpper()).ToList()
            };
        }

        public IList<IList<string>> Explode(List<string> tuple)
        {
            return tuple
                .Select(element => new List<string> {element})
                .Cast<IList<string>>()
                .ToList();
        }
    }
}

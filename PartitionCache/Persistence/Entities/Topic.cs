using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Persistence.Entities
{
    public class Topic
    {
        public Topic()
        {
            Producers = new List<KeyVal>();
        }

        public DateTime SavedUtc { get; set; }
        public string Name { get; set; }
        public int PartitionCount { get; set; }
        public List<KeyVal> Producers { get; set; }
    }
}

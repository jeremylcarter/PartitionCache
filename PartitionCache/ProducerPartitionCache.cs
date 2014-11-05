using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache
{
    /// <summary>
    /// Producer and matching Partition Cache
    /// </summary>
    public class ProducerPartitionCache
    {
        public ProducerPartitionCache(string sender, int partition)
        {
            Sender = sender;
            Partition = partition;
        }
        public string Sender { get; set; }
        public int Partition { get; set; }
    }

}

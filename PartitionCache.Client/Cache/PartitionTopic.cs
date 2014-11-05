using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Client.Cache
{
    [Serializable]
    public class PartitionTopic
    {
        public PartitionTopic()
        {
        }
        public PartitionTopic(int partition, string topic)
        {
            Partition = partition;
            Topic = topic;
        }
        public int Partition { get; set; }
        public string Topic { get; set; }

    }
}

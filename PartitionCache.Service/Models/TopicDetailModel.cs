using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Service.Models
{
    public class TopicDetailModel
    {
        public string Name { get; set; }
        public int Partitions { get; set; }
        public int Count { get; set; }
        public long Throughput { get; set; }
        public int ThroughputPercent { get; set; }
    }
}

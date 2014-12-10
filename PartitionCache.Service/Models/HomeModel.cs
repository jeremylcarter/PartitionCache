using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartitionCache.Persistence.Entities;

namespace PartitionCache.Service.Models
{
    public class HomeModel
    {
        public int TopicCount { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime StartTime { get; set; }
        public List<TopicDetailModel> Topics { get; set; }
    }
}

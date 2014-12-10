using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache
{
    public enum LoadBalanceStrategy
    {
        Count = 0,
        Throughput = 1,
        Random = 2
    }
}

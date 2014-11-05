using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache
{
    /// <summary>
    /// Producer. A sender or identifier for processing
    /// </summary>
    public class Producer
    {
        public string Name { get; set; }
        public DateTime LastSubmittedUtc { get; set; }

    }

}

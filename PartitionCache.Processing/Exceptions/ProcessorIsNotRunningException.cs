using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Processing.Exceptions
{
    public class ProcessorIsNotRunningException: Exception
    {
        public ProcessorIsNotRunningException():
            base("Processor is not running. Call method Start() to process items and allow new items to be added.")
        {
        }
    }
}

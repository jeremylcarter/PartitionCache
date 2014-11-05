using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Processing.Exceptions
{
    public class PartitionNotFoundException : Exception
    {
        public PartitionNotFoundException(int partition)
            : base(String.Format("Partition {0} was not found in the array.", partition))
        {

        }
    }
}

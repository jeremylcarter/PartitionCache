using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Client
{
    public class TopicNotFoundException : Exception
    {
        public TopicNotFoundException(string topic) 
            : base(String.Format("The topic named '{0}' could not be found. Create the topic again.", topic))
        {
        }
    }
}

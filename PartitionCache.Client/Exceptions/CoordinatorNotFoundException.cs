using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Client
{
  public class CoordinatorNotFoundException : Exception
    {
      public CoordinatorNotFoundException(string address)
          : base(String.Format("The coordinator at '{0}' could not be found.", address))
        {
        }
    }
}

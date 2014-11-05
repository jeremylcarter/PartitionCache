using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Persistence
{
    public static class StringExtensions
    {
        public static Stream ToMemoryStream(this string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s ?? ""));
        }
    }
}

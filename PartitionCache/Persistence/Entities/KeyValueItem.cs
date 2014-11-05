using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Persistence.Entities
{
    [Serializable]
    public class KeyVal
    {
        public KeyVal()
        {
        }
        public KeyVal(int key, string value)
        {
            Key = key;
            Value = value;
        }
        public int Key { get; set; }
        public string Value { get; set; }

    }
}

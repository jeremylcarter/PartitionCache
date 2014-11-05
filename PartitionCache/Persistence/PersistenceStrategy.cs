using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Persistence
{
    public class PersistenceStrategy
    {
        public PersistenceStrategy()
        {
            Enabled = true;
            LoadOnStartup = true;
            Interval = TimeSpan.FromSeconds(120);
            Mode = PersistenceMode.Interval;
        }

        public PersistenceMode Mode { get; set; }
        public bool Enabled { get; set; }
        public bool LoadOnStartup { get; set; }
        public TimeSpan Interval { get; set; }

    }

    public enum PersistenceMode
    {
        Interval = 0,
        Eventual = 1
    }
}

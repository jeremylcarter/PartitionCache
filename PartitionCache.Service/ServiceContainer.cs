using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartitionCache.Persistence;

namespace PartitionCache.Service
{
    public static class ServiceContainer
    {
        public static Coordinator Coordinator { get; set; }
        public static DateTime LastPersist { get; set; }
        public static DateTime LastVacuum { get; set; }
        public static IPersistenceProvider PersistenceProvider { get; set; }
        public static PersistenceStrategy PersistenceStrategy { get; set; }
        public static DateTime StartTime { get; set; }
    }
}

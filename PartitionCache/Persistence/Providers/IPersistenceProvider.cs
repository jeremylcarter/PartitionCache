using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PartitionCache.Persistence.Entities;

namespace PartitionCache.Persistence
{
    public interface IPersistenceProvider
    {
        string FileExtension { get;}
        bool Save(string topic, List<Partition> partitions);
        bool Save(Topic topic);
        Topic Load(string topic);

    }
}

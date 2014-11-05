using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Client.Cache
{
    public class PartitionMemoryCache
    {
        private ObjectCache cache;

        public PartitionMemoryCache()
        {
            cache = MemoryCache.Default;
        }

        public void AddItem(string topic, int partition, string producer)
        {
            var hash = String.Format("{0}|{1}", topic, producer);
            var cacheItem = new CacheItem(hash, new PartitionTopic(partition, topic));
            AddItem(cacheItem, 15);
        }
        public void AddItem(CacheItem item, int span)
        {
            CacheItemPolicy cp = new CacheItemPolicy();
            cp.SlidingExpiration.Add(TimeSpan.FromMinutes(span));
            cache.Add(item, cp);
        }

        public bool Contains(string topic, string producer)
        {
            var hash = String.Format("{0}|{1}", topic, producer);
            return cache.Contains(hash);
        }

        public PartitionTopic GetItem(string topic, string producer)
        {
            try
            {
                var hash = String.Format("{0}|{1}", topic, producer);
                return (PartitionTopic)cache.Get(hash);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Object GetItem(string key)
        {
            return cache.Get(key);
        }
    }
}

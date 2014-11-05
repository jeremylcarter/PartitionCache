using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Processing
{
    public class ConcurrentUniqueQueue<TQueue, TKey>
    {
        public static ConcurrentQueue<TQueue> Queue;
        public static ConcurrentDictionary<TKey, TKey> Keys;

        public ConcurrentUniqueQueue()
        {
            Queue = new ConcurrentQueue<TQueue>();
            Keys = new ConcurrentDictionary<TKey, TKey>();

        }

        public void AddKey(TKey key)
        {
            if (Keys.ContainsKey(key) == false)
            {
                Keys.TryAdd(key, key);
            }
        }

        public bool KeyExists(TKey key)
        {
            return Keys.ContainsKey(key);
        }

        public void RemoveKey(TKey key)
        {
            try
            {
                if (!Keys.ContainsKey(key)) return;
                TKey returnVal;
                Keys.TryRemove(key, out returnVal);
            }
            catch (Exception)
            {
            }
        }

        public void Enqueue(TQueue item)
        {
            Queue.Enqueue(item);
        }

        public bool Enqueue(TQueue item, TKey key)
        {
            if (!KeyExists(key))
            {
                Queue.Enqueue(item);
                AddKey(key);
                return true;
            }

            return false;
        }

        public bool TryDequeue(out TQueue result)
        {
            return Queue.TryDequeue(out result);
        }

        public bool TryPeek(out TQueue result)
        {
            return Queue.TryPeek(out result);
        }


        public int Count()
        {
            return Queue.Count;
        }

        public void ClearAll()
        {
            try
            {

                if (Queue.Count > 0)
                {
                    for (var i = 1; i < Queue.Count; i++)
                    {
                        try
                        {
                            TQueue temp;
                            Queue.TryDequeue(out temp);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                if (Keys.Count > 0)
                {
                    Keys.Clear();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}

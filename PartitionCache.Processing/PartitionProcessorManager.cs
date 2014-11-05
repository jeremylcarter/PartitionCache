using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache.Processing
{
    public class PartitionProcessorManager<TProcessorBase, TQueueItem, TQueueItemProperty> where TProcessorBase: PartitionProcessorBase<TQueueItem, TQueueItemProperty> ,new()
    {
        public ConcurrentDictionary<int, PartitionProcessorBase<TQueueItem, TQueueItemProperty>> Processors { get; set; }
        public PartitionProcessorManager(int partitionCount, Expression<Func<TQueueItem, TQueueItemProperty>> exp)
        {

            Processors = new ConcurrentDictionary<int, PartitionProcessorBase<TQueueItem, TQueueItemProperty>>();

            for (int i = 1; i < (partitionCount + 1); i++)
            {
                var processorBase = new TProcessorBase();
                processorBase.Partition = i;
                processorBase.Expression = exp;
                Processors.TryAdd(i, processorBase);
            }
        }

        public void Submit(TQueueItem item, int partition)
        {
            if (Processors.ContainsKey(partition))
            {
                if (!Processors[partition].IsRunning)
                {
                    Processors[partition].Start();
                }

                Processors[partition].AddItem(item);
            }
        }
    }
}

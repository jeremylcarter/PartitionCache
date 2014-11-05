using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartitionCache.Persistence.Entities;

namespace PartitionCache
{
    /// <summary>
    /// Topic Coordinator. Coordinates new producers to existing partitions
    /// </summary>
    public sealed class TopicCoordinator
    {
        private Random _random;

        public int MaxPartitions { get; set; }

        private static Object _partitionLock = new Object();

        public string Topic { get; set; }
        public ConcurrentDictionary<int, Partition> Partitions { get; set; }
        public ConcurrentDictionary<string, ProducerPartitionCache> ProducerCache { get; set; }

        public TopicCoordinator(string topic = "default", int maxPartitionCount = 16)
        {
            Topic = topic;
            MaxPartitions = maxPartitionCount;
            _random = new Random(DateTime.Now.Millisecond);
            Partitions = new ConcurrentDictionary<int, Partition>();
            ProducerCache = new ConcurrentDictionary<string, ProducerPartitionCache>();

            Setup();
        }

        public List<PartitionDetail> GetPartitionDetails()
        {

            var detailList = new List<PartitionDetail>();

            if (Partitions.Count >= 1)
            {
                for (int i = 1; i < (MaxPartitions + 1); i++)
                {
                    var detail = new PartitionDetail();
                    detail.Count = Partitions[i].ProducerCount;
                    detail.Number = Partitions[i].Number;
                    detailList.Add(detail);
                }
            }

            return detailList;
        }

        public PartitionDetail GetAvailablePartition()
        {
            try
            {
                var detailList = GetPartitionDetails();

                var free = detailList.Where(i => i.Count == 0).ToList();
                if (free.Count == 1)
                {
                    return free.FirstOrDefault();
                }
                else if (free.Count >= 2)
                {
                    var random = _random.Next(1, free.Count - 1);
                    return free.ElementAt(random - 1);
                }

                if (free.Count == 0)
                {
                    var lowest = detailList.OrderBy(i => i.Count).Take(5).ToList();
                    var random = _random.Next(0, 4);
                    return lowest.ElementAt(random - 1);
                }

                return GetRandomPartition();
            }
            catch (Exception ex)
            {
                return GetRandomPartition();
            }

        }

        private PartitionDetail GetRandomPartition()
        {

            try
            {
                var number = _random.Next(1, this.MaxPartitions);
                return new PartitionDetail() { Number = number };
            }
            catch (Exception ex)
            {
                return new PartitionDetail() { Number = 1 };
            }

        }

        public bool Setup()
        {

            if (Partitions.Count == 0 && ProducerCache.Count == 0)
            {

                for (int i = 1; i < (MaxPartitions + 1); i++)
                {
                    CreatePartition(i);
                }
            }
            return true;
        }

        private bool AddToProducerCache(string producer, int partition)
        {

            try
            {
                    return ProducerCache.TryAdd(producer, new ProducerPartitionCache(producer, partition));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool UpdateProducerToPartition(string producer, int partition)
        {
            try
            {
                Task.Run(() =>
                {
                    Partitions[partition].Producers[producer].LastSubmittedUtc = DateTime.UtcNow;
                });
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool AddProducerToPartition(string producer, int partition)
        {

            try
            {
                Partition part = null;
                Partitions.TryGetValue(partition, out part);
                if (part != null)
                {
                    var added = false;
                    lock (part)
                    {
                        added = part.Producers.TryAdd(producer, new Producer() { Name = producer, LastSubmittedUtc = DateTime.UtcNow });
                    }

                    if (added)
                    {
                        AddToProducerCache(producer, partition);
                    }
                }
                else
                {
                    CreatePartition(partition);
                    AddProducerToPartition(producer, partition);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;

        }

        private bool CreatePartition(int index)
        {

            var partition = new Partition() { CreatedUtc = DateTime.UtcNow, Number = index };
            lock (this.Partitions)
            {
                if (!this.Partitions.ContainsKey(index))
                {
                    lock (_partitionLock)
                    {
                        this.Partitions.TryAdd(index, partition);
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }

            return false;

        }

        private int AssignNewPartition(string sender)
        {

            var returnPartitionNumber = 0;

            if (!ProducerCache.ContainsKey(sender))
            {
                // Sender is not in Dictionary cache

                var availablePartition = GetAvailablePartition();
                var availablePartitionNumber = availablePartition.Number;

                if (Partitions.ContainsKey(availablePartitionNumber))
                {

                    // Assign this producer to this partition
                    Console.WriteLine("Assigning producer '{1}' to partition {0}", availablePartitionNumber, sender);

                    returnPartitionNumber = availablePartitionNumber;

                    if (AddProducerToPartition(sender, availablePartitionNumber))
                    {
                        AddToProducerCache(sender, returnPartitionNumber);
                    }

                }
                else
                {
                    // Create a new partition and assign this producer to it

                    var created = CreatePartition(availablePartitionNumber);

                    returnPartitionNumber = availablePartitionNumber;

                    if (AddProducerToPartition(sender, availablePartitionNumber))
                    {
                        AddToProducerCache(sender, returnPartitionNumber);
                    }

                }
            }

            return returnPartitionNumber;
        }

        public int GetOrAssignPartition(string sender)
        {
            var returnPartitionNumber = 0;

            try
            {
                var inDictionary = ProducerCache.ContainsKey(sender);
                if (inDictionary)
                {

                    ProducerPartitionCache producerPartitionCache = null;
                    var tryGetValue = ProducerCache.TryGetValue(sender, out producerPartitionCache);
                    if (tryGetValue && producerPartitionCache != null)
                    {

                        Console.WriteLine("Sender {0} belongs in Partition {1}", sender, producerPartitionCache.Partition);

                        // Update
                        returnPartitionNumber = producerPartitionCache.Partition;
                        UpdateProducerToPartition(sender, producerPartitionCache.Partition);
                        return producerPartitionCache.Partition;

                    }
                    else
                    {

                        // Clear the cache for this producer
                        try
                        {
                            ProducerPartitionCache oldValue = null;
                            ProducerCache.TryRemove(sender, out oldValue);
                        }
                        catch (Exception ex)
                        {
                        }

                        return AssignNewPartition(sender);

                    }
                }
                else
                {

                    // Full scan required first before we assign a new partition
                    foreach (var partition in Partitions)
                    {
                        var exists = partition.Value.Producers.ContainsKey(sender);
                        if (exists)
                        {
                            AddToProducerCache(sender, returnPartitionNumber);
                            return partition.Key;
                        }

                    }

                    // Assign a new partition
                    return AssignNewPartition(sender);
                }

            }
            catch (Exception ex)
            {
                return GetRandomPartition().Number;
            }

            return returnPartitionNumber;
        }

        public static explicit operator Persistence.Entities.Topic(TopicCoordinator coordinator)
        {
            var returnValue = new Persistence.Entities.Topic();
            returnValue.Name = coordinator.Topic;

            try
            {
                if (!coordinator.Partitions.IsEmpty && coordinator.Partitions.Count >= 1)
                {
                    returnValue.PartitionCount = coordinator.Partitions.Count;

                    foreach (var partition in coordinator.Partitions)
                    {
                        try
                        {
                            var pNum = partition.Key;
                            foreach (var producer in partition.Value.Producers)
                            {
                                returnValue.Producers.Add(new KeyVal(pNum, producer.Key));
                            }
                        }
                        catch (Exception)
                        {
                        }
                       
                    }
                }

                return returnValue;
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
    }
}

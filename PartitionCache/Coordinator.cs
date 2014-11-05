using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartitionCache
{

    /// <summary>
    /// Coordinator. Coordinates and manages multiple topic coordinators.
    /// </summary>
    public class Coordinator
    {
        private static Object _topicLock = new Object();
        public ConcurrentDictionary<string, TopicCoordinator> Topics { get; set; }

        public Coordinator()
        {
            Topics = new ConcurrentDictionary<string, TopicCoordinator>();
        }
        public List<string> ListAllTopics()
        {
            try
            {
                return Topics.Keys.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }


        public List<PartitionDetail> ListAllTopicDetail(string topic)
        {
            try
            {
                return Topics[topic].GetPartitionDetails();

            }
            catch (Exception)
            {
                return null;
            }
        }


        public bool TopicExists(string topic)
        {
            return Topics.ContainsKey(topic);
        }
        public TopicCoordinator GetTopicCoordinator(string topic)
        {
            if (Topics.ContainsKey(topic))
            {
                return Topics[topic];
            }
            else
            {
                var created = CreateTopicCoordinator(topic);
                if (created)
                {
                    return Topics[topic];
                }
            }

            return null;
        }
        public bool CreateTopicCoordinator(string topic, int partitions = 16)
        {
            try
            {
                if (Topics.ContainsKey(topic))
                {
                    return true;
                }
                else
                {
                    var coordinator = new TopicCoordinator(topic, partitions);
                    lock (_topicLock)
                    {
                        Topics.TryAdd(topic, coordinator);
                    }
                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

        public int PartitionProducer(string topic, string producer)
        {
            try
            {
                if (TopicExists(topic))
                {
                    return Topics[topic].GetOrAssignPartition(producer);
                }
            }
            catch (Exception)
            {
                return 0;
            }

            return 0;
        }

        public bool AddProducerToExistingTopic(string topic, int partition, string producer)
        {
            if (Topics.ContainsKey(topic))
            {
                Topics[topic].AddProducerToPartition(producer, partition);
            }
            else
            {
                return false;
            }

            return false;
        }
    }
}

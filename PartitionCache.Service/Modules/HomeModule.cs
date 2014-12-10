using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using PartitionCache.Service.Models;

namespace PartitionCache.Service
{
    public class HomeModule : Nancy.NancyModule
    {
        public HomeModule()
        {
            Get["/", runAsync: true] = async (parameters, ct) =>
            {
                var model = new HomeModel();
                model.StartTime = ServiceContainer.StartTime.ToLocalTime();
                model.StartTimeUtc = ServiceContainer.StartTime;
                model.Topics = new List<TopicDetailModel>();

                var topics = ServiceContainer.Coordinator.Topics.ToList().AsReadOnly();

                foreach (var t in topics)
                {
                    try
                    {
                        var details = t.Value.GetPartitionDetails().ToList();
                        model.Topics.Add(new TopicDetailModel()
                        {
                            Count = details.Sum(i => i.Count),
                            Partitions = t.Value.MaxPartitions,
                            Name = t.Key,
                            Throughput = details.Sum(i => i.Throughput)
                        });
                    }
                    catch (Exception)
                    {
                    }
                }

                var totalThroughput = Convert.ToDouble((model.Topics.Sum(i => i.Throughput)));

                foreach (var t in model.Topics)
                {
                    try
                    {
                        if (t.Throughput != 0 && t.Count >= 1)
                        {
                            var percent = (Convert.ToDouble(t.Throughput) / totalThroughput * 100);
                            t.ThroughputPercent = Convert.ToInt32(percent);
                        }
                    }
                    catch (Exception)
                    {
                        t.ThroughputPercent = 0;
                    }
                }

                model.TopicCount = topics.Count;

                return View["Home", model];
            };

            Get["/list", runAsync: true] = async (parameters, ct) =>
            {
                return Response.AsJson(ServiceContainer.Coordinator.ListAllTopics().AsReadOnly());
            };

            Get["/statistics", runAsync: true] = async (parameters, ct) =>
            {
                var topics = ServiceContainer.Coordinator.Topics.ToList().AsReadOnly();
                return Response.AsJson(topics.Select(i => new { Topic = i.Key, Partitions = ServiceContainer.Coordinator.ListAllTopicDetail(i.Key) }));
            };

            Get["/statistics/{topic}", runAsync: true] = async (parameters, ct) =>
            {
                var name = (string)parameters.topic;
                name = name.ToLowerInvariant();
                return Response.AsJson(ServiceContainer.Coordinator.ListAllTopicDetail(name));
            };
        }
    }
}

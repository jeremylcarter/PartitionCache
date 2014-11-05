using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace PartitionCache.Service
{
    public class HomeModule : Nancy.NancyModule
    {
        public HomeModule()
        {
            Get["/", runAsync: true] = async (parameters, ct) =>
            {
                return Response.AsJson(ServiceContainer.Coordinator.ListAllTopics());
            };

            Get["/statistics", runAsync: true] = async (parameters, ct) =>
            {
                var topics = ServiceContainer.Coordinator.Topics;
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

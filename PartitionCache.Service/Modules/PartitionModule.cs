using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace PartitionCache.Service
{
    public class PartitionModule
        : Nancy.NancyModule
    {
        public PartitionModule()
            : base("/{topic}")
        {
            Get["/add/{producer}", runAsync: true] = async (parameters, ct) =>
            {
                if (!String.IsNullOrEmpty((string)parameters.producer))
                {
                    var producer = (string)parameters.producer;
                    var topic = (string)parameters.topic;

                    if (ServiceContainer.Coordinator.TopicExists(topic))
                    {
                        var partition = ServiceContainer.Coordinator.PartitionProducer(topic, producer);
                        return Response.AsJson(partition);
                    }

                }
                return HttpStatusCode.BadRequest;

            };

            Get["/list", runAsync: true] = async (parameters, ct) =>
            {
                var topic = (string)parameters.topic;
                return Response.AsJson(ServiceContainer.Coordinator.Topics[topic].GetPartitionDetails());
            };
        }
    }
}

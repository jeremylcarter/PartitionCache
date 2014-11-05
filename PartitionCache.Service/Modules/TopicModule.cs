using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace PartitionCache.Service
{
    public class TopicModule : Nancy.NancyModule
    {
        public TopicModule()
            : base("/topics")
        {
            Get["/exists/{name}", runAsync: true] = async (parameters, ct) =>
            {
                var name = (string)parameters.name;
                name = name.ToLowerInvariant();

                var exists = ServiceContainer.Coordinator.TopicExists(name);
                if (exists)
                {
                    return HttpStatusCode.OK;
                }
                else
                {
                    return HttpStatusCode.NotFound;
                }                

            };

            Get["/partitions/{name}", runAsync: true] = async (parameters, ct) =>
            {
                var name = (string)parameters.name;
                name = name.ToLowerInvariant();

                var exists = ServiceContainer.Coordinator.TopicExists(name);
                if (exists)
                {
                    return Response.AsJson(ServiceContainer.Coordinator.Topics[name].MaxPartitions);
                }
                else
                {
                    return HttpStatusCode.NotFound;
                }

            };
            Get["/add/{name}/{partitions?}", runAsync: true] = async (parameters, ct) =>
            {

                var param = (Nancy.DynamicDictionary)parameters;
                if (param != null && param.Keys.Count == 2)
                {
                    var name = (string)parameters.name;
                    name = name.ToLowerInvariant();
                    var partitions = (int)parameters.partitions;

                    var exists = ServiceContainer.Coordinator.TopicExists(name);
                    if (exists) return HttpStatusCode.OK;

                    var created = ServiceContainer.Coordinator.CreateTopicCoordinator(name, partitions);

                    if (created)
                    {
                        return HttpStatusCode.OK;
                    }
                    else
                    {
                        return HttpStatusCode.BadRequest;
                    }        

                }
                else
                {
                    var name = (string)parameters.name;
                    name = name.ToLowerInvariant();

                    var exists = ServiceContainer.Coordinator.TopicExists(name);
                    if (exists) return HttpStatusCode.OK;

                    var created =ServiceContainer.Coordinator.CreateTopicCoordinator(name);

                    if (created)
                    {
                        return HttpStatusCode.OK;
                    }
                    else
                    {
                        return HttpStatusCode.BadRequest;
                    }       

                }

                return HttpStatusCode.BadRequest;
            };

            Get["/list", runAsync: true] = async (parameters, ct) => Response.AsJson(ServiceContainer.Coordinator.ListAllTopics());
        }
    }
}

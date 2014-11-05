using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PartitionCache.Client.Cache;

namespace PartitionCache.Client
{
    public class PartitionCacheClient
    {
        public PartitionMemoryCache Cache { get; set; }
        public bool EnableCache { get; set; }
        public Uri BaseAddress { get; set; }

        private HttpClient _client;
        public PartitionCacheClient(string address, int port, bool cache = true)
        {
            EnableCache = cache;
            BaseAddress = new Uri(String.Format("http://{0}:{1}/", address, port));

            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = null,
                UseDefaultCredentials = false,
                UseProxy = false,
            };

            _client = new HttpClient(handler) { BaseAddress = this.BaseAddress };
            _client.Timeout = TimeSpan.FromSeconds(1);
            if (cache)
            {
                Cache = new PartitionMemoryCache();
            }
        }
        public PartitionCacheClient(int port = 7070, bool cache = true)
        {
            EnableCache = cache;
            BaseAddress = new Uri(String.Format("http://localhost:{0}/", port));

            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = null,
                UseDefaultCredentials = false,
                UseProxy = false,
            };

            _client = new HttpClient(handler) { BaseAddress = this.BaseAddress };
            if (cache)
            {
                Cache = new PartitionMemoryCache();
            }
        }

        public async Task<List<string>> Topics()
        {
            try
            {
                _client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await _client.GetAsync(String.Format("topics/list"));
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<List<string>>(jsonString);
                    return model;
                }

                return new List<string>();
            }
            catch (Exception)
            {
                throw new CoordinatorNotFoundException(this.BaseAddress.ToString());
            }

        }

        public async Task<int> AddProducer(string topic, string producer)
        {
            try
            {

                if (EnableCache && Cache != null)
                {
                    try
                    {
                        if (Cache.Contains(topic, producer))
                        {
                            var item = Cache.GetItem(topic, producer);
                            if (item != null) return item.Partition;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                _client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await _client.GetAsync(String.Format("{0}/add/{1}", topic, producer));
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<int>(jsonString);
                    if (model == 0)
                    {
                        throw new TopicNotFoundException(topic);
                    }
                    else
                    {
                        if (EnableCache && Cache != null)
                        {
                            Cache.AddItem(topic, model, producer);
                        }
                        return model;
                    }

                }

                throw new TopicNotFoundException(topic);
            }
            catch (Exception)
            {
                throw new TopicNotFoundException(topic);
            }

        }

        public async Task<bool> TopicExists(string topic)
        {
            try
            {
                _client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await _client.GetAsync("topics/add/" + topic);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;

            }
            catch (Exception)
            {

                throw new CoordinatorNotFoundException(this.BaseAddress.ToString());
            }

        }

        public async Task<int> TopicPartitionCount(string topic)
        {
            try
            {
                _client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await _client.GetAsync("topics/partitions/" + topic);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<int>(jsonString);
                    if (model == 0)
                    {
                        throw new TopicNotFoundException(topic);
                    }

                    return model;
                }

                throw new TopicNotFoundException(topic);

            }
            catch (Exception)
            {

                throw new CoordinatorNotFoundException(this.BaseAddress.ToString());
            }

        }

        public async Task<bool> AddTopic(string topic)
        {
            try
            {
                _client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await _client.GetAsync("topics/add/" + topic);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                throw new CoordinatorNotFoundException(this.BaseAddress.ToString());

            }
            catch (Exception)
            {

                throw new CoordinatorNotFoundException(this.BaseAddress.ToString());
            }

        }
    }
}

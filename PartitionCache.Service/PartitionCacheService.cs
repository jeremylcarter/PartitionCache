using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Nancy;
using Nancy.Hosting.Self;
using PartitionCache.Persistence;

namespace PartitionCache.Service
{
    public partial class PartitionCacheService : ServiceBase
    {
        public NancyHost Host;
        public string DataPath;
        public System.Timers.Timer Timer;
        public PartitionCacheService()
        {
            InitializeComponent();

            ServiceContainer.Coordinator = new Coordinator();
            ServiceContainer.StartTime = DateTime.UtcNow;
            ServiceContainer.LastPersist = DateTime.UtcNow.AddMinutes(-5);
            ServiceContainer.LastVacuum = DateTime.UtcNow.AddMinutes(-5);
            ServiceContainer.PersistenceProvider = new XmlPersistence();
            ServiceContainer.PersistenceStrategy = new PersistenceStrategy();
            DataPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Timer = new Timer(ServiceContainer.PersistenceStrategy.Interval.TotalMilliseconds);
            if (ServiceContainer.PersistenceStrategy.Enabled)
            {
                System.AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                if (ServiceContainer.PersistenceStrategy.Mode == PersistenceMode.Eventual)
                {
                    Timer.Interval = 30000; // 30 seconds
                }
                Timer.Elapsed += Timer_Elapsed;
                Timer.Enabled = true;
            }

        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if ((DateTime.UtcNow - ServiceContainer.LastPersist).TotalSeconds > 5)
            {
                TryPersist();
            }
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                TryPersist();
                ServiceContainer.LastPersist = DateTime.UtcNow;
            });
        }

        public void TryPersist()
        {
            try
            {
                if (ServiceContainer.PersistenceStrategy != null && ServiceContainer.PersistenceProvider != null)
                {
                    if (ServiceContainer.PersistenceStrategy.Enabled)
                    {
                        foreach (var t in ServiceContainer.Coordinator.Topics)
                        {
                            try
                            {
                                var topic = (Persistence.Entities.Topic)t.Value;
                                if (topic != null)
                                {
                                    ServiceContainer.PersistenceProvider.Save(topic);
                                }
                            }
                            catch (Exception)
                            {
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        Uri LoadUri()
        {
            try
            {
                var defaultDomain = "localhost";
                var defaultPort = 7070;

                if (ConfigurationManager.AppSettings["IPAddress"] != null)
                {
                    defaultDomain = ConfigurationManager.AppSettings["IPAddress"];
                }
                else
                {
                    defaultDomain = PartitionCache.Service.Properties.Settings.Default.IPAddress;
                }

                if (ConfigurationManager.AppSettings["Port"] != null)
                {
                    defaultPort = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                }
                else
                {
                    defaultPort = PartitionCache.Service.Properties.Settings.Default.Port;
                }

                return new Uri("http://" + defaultDomain + ":" + defaultPort);
            }
            catch (Exception)
            {
                return new Uri("http://localhost:7070");
            }
        }

        void LoadExistingPersistence()
        {
            try
            {
                // Load up existing
                string[] files = System.IO.Directory.GetFiles(DataPath, "*." + ServiceContainer.PersistenceProvider.FileExtension);

                foreach (var s in files)
                {
                    try
                    {
                        var topic = ServiceContainer.PersistenceProvider.Load(s);
                        if (topic != null)
                        {
                            if (topic.PartitionCount >= 1 && topic.Producers.Count >= 1)
                            {
                                // Create it and add the items
                                ServiceContainer.Coordinator.CreateTopicCoordinator(topic.Name, topic.PartitionCount);
                                foreach (var producer in topic.Producers)
                                {
                                    ServiceContainer.Coordinator
                                        .AddProducerToExistingTopic(topic.Name, producer.Key,
                                        producer.Value);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error loading topic from '{0}'.", s);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading persistence file '{0}'", ex.Message);
            }

        }
        protected override void OnStart(string[] args)
        {

            bool exists = System.IO.Directory.Exists(DataPath);
            if (!exists) System.IO.Directory.CreateDirectory(DataPath);

            if (ServiceContainer.PersistenceStrategy != null && ServiceContainer.PersistenceProvider != null)
            {
                if (ServiceContainer.PersistenceStrategy.Enabled && ServiceContainer.PersistenceStrategy.LoadOnStartup)
                {
                    LoadExistingPersistence();
                }
            }

            var defaultUri = LoadUri();
          
            var config = new HostConfiguration();
            config.RewriteLocalhost = true;
            config.UrlReservations = new UrlReservations() { CreateAutomatically = true };

            Host = new NancyHost(defaultUri, new DefaultNancyBootstrapper(), config);
            Host.Start();

        }

        protected override void OnStop()
        {
            if (Host != null)
            {
                Host.Stop();
            }

            TryPersist();
            ServiceContainer.LastPersist = DateTime.UtcNow;
        }
    }
}

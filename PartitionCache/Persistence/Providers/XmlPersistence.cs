using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PartitionCache.Persistence.Entities;

namespace PartitionCache.Persistence
{
    public class XmlPersistence : IPersistenceProvider
    {
        public string FileExtension
        {
            get { return "xml"; }
        }

        public bool Save(string topic, List<Partition> partitions)
        {
            var entity = new Topic() { Name = topic, PartitionCount = partitions.Count };
            foreach (var partition in partitions)
            {
                foreach (var producers in partition.Producers)
                {
                    entity.Producers.Add(new KeyVal(partition.Number, producers.Key));
                }
            }
            return Save(entity);
        }

        public bool Save(Topic topic)
        {
            try
            {
                var fileName = CreateFileName(topic.Name);
                topic.SavedUtc = DateTime.UtcNow;

                MemoryStream memStream = new MemoryStream();

                using (XmlTextWriter textWriter = new XmlTextWriter(memStream, Encoding.Unicode))
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Topic));
                    serializer.Serialize(textWriter, topic);

                    memStream = textWriter.BaseStream as MemoryStream;
                }
                if (memStream != null)
                {
                    WriteFile(fileName, memStream.ToArray());

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Topic Load(string topic)
        {
            try
            {
                var fileName = CreateFileName(topic);

                if (System.IO.File.Exists(topic))
                {
                    fileName = topic;
                }

                var xmlString = ReadFileAsString(fileName);

                if (string.IsNullOrWhiteSpace(xmlString))
                    return null;
                
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Topic));
                using (StringReader sr = new StringReader(xmlString))
                    return (Topic)serializer.Deserialize(sr);

            }
            catch (Exception)
            {
                return null;
            }
        }

        #region FileSystem

        private byte[] ReadFile(string fileName)
        {
            try
            {
                return System.IO.File.ReadAllBytes(fileName);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            return null;
        }

        private String ReadFileAsString(string fileName)
        {
            try
            {
                return System.IO.File.ReadAllText(fileName, Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            return null;
        }
        private bool WriteFile(string fileName, byte[] bytes)
        {
            try
            {
                System.IO.File.WriteAllBytes(fileName, bytes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool CreateFile(string fileName)
        {
            try
            {
                System.IO.File.Create(fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string CreateFileName(string topic)
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", String.Format("{0}.{1}", topic, this.FileExtension));
        }

        #endregion

    }
}

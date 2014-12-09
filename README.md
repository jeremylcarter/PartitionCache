![Logo](http://i.imgur.com/X87oJnP.png)

## PartitionCache : Split up your workload

PartitionCache is a simple persistent producer partition cache. What does that mean? Well it means it remembers what partition a producer belongs to and allows you to silo your workload into multiple partitions to avoid any multi-threaded headaches or concurrency issues. PartitionCache does not store any events or items that require processing, rather it acts as a persistent store of the partition number each item belongs to.

## Rationale

PartitionCache fills the void of persistent Dictionary<int, string> where int is the partition and string is the name of the producer. This allows producers or unique keys to have a persistent unique integer assigned to them for their lifetime. PartitionCache gets out of your way and allows you to model your code to eventually migrate to Kafka, Azure Event Hubs or Amazon Kinesis.

I am using PartitionCache to provide an extremely lightweight Kafka like producer partitioner. When processing items you want to ensure that you aren't processing DeviceId1234 items over the top of each other. Perhaps an easy solution would be to use an event streaming software and aggregate the events. In most cases though you just need a quick fix and PartitionCache is just that. You'll be up and running within minutes and you'll be able to increase your processing throughput whilst ensuring items are processed on time.

## Terms

*Producer* 			: An entity, device or any unique identifier that whereby work is aggregated.<br/>
*Partition*			: A group of Producers that are assigned a unique integer for their lifetime.<br/>
*Topic*				: A group of Partitions. A topic is named using a lowercase culture invariant string.<br/>
*Topic Coordinator* : Manages a topic and assigns producers to partitions based on a partition with the lowest producer count value.<br/>
*Coordinator*		: Manages multiple Topic Coordinators<br/>

## Usage Example

Lets say you have a 500 telemetry devices and you want to process or re-process information as quickly as possible. The best idea would be to use Azure Event Hubs or Amazon Kinesis. If you cannot use these services then install PartitionCache on your main processing server or run it on a small linux box using Mono. 

Use the PartitionCache.Client library and ensure you include Json.net from Nuget or your own source. <br/>
In your processing code you could partition the items you need to process by the telemetry device id, perhaps this is the IMEI or another unique identifier. Once you have this done you can utilise the code in PartitionCache.Processing as a base to setup the processing of your partitioned items. Each partition is capable of executing items continuously one by one, or if you care to write more complex code you could process all items in a partition. PartitionCache does not store the items you need to process.

![Overview](http://i.imgur.com/jhnLvrK.png)

![Overview2](http://i.imgur.com/pjLoqVI.png)

## Client Usage

```c#
var topic = "mybackgroundprocessor";

var partitionClient = new PartitionCacheClient(); // Overload for custom URI

// Determine if the topic exists. Topics are created with a default of 16 partitions
// You will be surprised how well just 16 partitions will improve processing throughput
var topicExists = await partitionClient.TopicExists(topic);
if (!topicExists) {
	await partitionClient.AddTopic(topic);
}
// Add a producer to topic with key of the person number
// The producer name must be something that you aggregate in your processing code
// or the producer name can be a Guid or Integer ie. a primary key from a database.
var partition = await partitionClient.AddProducer(topic, person.PersonNumber);

```

## Requirements

Currently PartitionCache uses .net 4.5 but it can be modified to use 4.0 easily.

## Roadmap

* Improve performance
* Add statistic display pages to hosted http server
* Add ability to override currently allocated partition value
* Support for partitions larger than 256
* Support for basic load balancing of partitions instead of currently LRU cache style balancing

## Tags

Multiple Partitions, Parallelism, Scaling Out, Cache, Key Value Cache, Threading, C#

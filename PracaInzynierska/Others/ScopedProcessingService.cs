using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Logging;
using PracaInzynierska.Others;

namespace PracaInzynierska.Others
{
    internal interface IScopedProcessingService
    {
        void DoWork();
    }

    internal class ScopedProcessingService: IScopedProcessingService
    {
        private const string EventHubConnectionString = "Endpoint=sb://ihsuprodbyres047dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=ib4HWDWL7wqBkzkGlDxhhfQ5n7ujqe3AVrWAYMQdPzA=;EntityPath=iothub-ehub-pracainzyn-919511-4d9c84981b";
        private const string EventHubName = "iothub-ehub-pracainzyn-919511-4d9c84981b";
        private const string StorageContainerName = "messages";

        private static readonly string StorageConnectionString =
            "DefaultEndpointsProtocol=https;AccountName=inzstorage;AccountKey=uA6F6BMRoHGP3RpfL2KVIF2shhawP3drGdRsZkPVJXYCfSTCq9VoL6A0uMKNKwpLLfke4UsQ2mUYFHJwGaQ/Ag==;EndpointSuffix=core.windows.net";

        private readonly ILogger _logger;

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger)
        {
            _logger = logger;
        }

        public  void DoWork()
        {
            var eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName); 

            // Registers the Event Processor Host and starts receiving messages
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();
            _logger.LogInformation("Scoped Processing Service is working.");
            //await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}

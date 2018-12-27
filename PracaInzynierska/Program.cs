using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using PracaInzynierska.Others;

namespace PracaInzynierska
{
    public class Program
    {
        private const string EventHubConnectionString = "Endpoint=sb://inzynierka.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wSzgim++b53M2I6/zGBZQIXfcBWlkDf+HFjsT/TLF60=";
        private const string EventHubName = "inzynierkaeventhub";
        private const string StorageContainerName = "inz";
        private const string StorageAccountName = "inzynierka";
        private const string StorageAccountKey = "7aAk2oKO9j0/DXAE3Tn1Cbddm1B7oMWIavP0ots0iqxpWEdDpZ+aeDCxUduhODxjDIGb2yiFgJDjCqPPHdCSpQ==";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
        private readonly ISignalR _signalRController;

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Registering EventProcessor...");
            var eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

            Console.WriteLine("Receiving. Press ENTER to stop worker.");
            Console.ReadLine();

            // Disposes of the Event Processor Host
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }

        public static void Main(string[] args)
        {
            MainAsync(args);
            CreateWebHostBuilder(args).Build().Run();

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        
    }
}

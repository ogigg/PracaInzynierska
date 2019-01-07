using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PracaInzynierska.Hubs;
using PracaInzynierska.Others;

namespace PracaInzynierska
{
    public interface ISimpleEventProcessor
    {
        Task CloseAsync(PartitionContext context, CloseReason reason);
        Task OpenAsync(PartitionContext context);
        Task ProcessErrorAsync(PartitionContext context, Exception error);
        Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages);
    }

    public class SimpleEventProcessor : IEventProcessor, ISimpleEventProcessor
    {
        private readonly ISignalR _signalRController;
        private static IHubContext<IoTSignalRHub> _hubContext;

        public ILogger<SimpleEventProcessor> Logger { get; }

        public SimpleEventProcessor()
        {
        }
        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Logger.LogInformation("Processor Shutting Down. Partition.");
            //Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            _hubContext.Clients.All.SendAsync("sendToAll", "signalREventProcessor", "SimpleEventProcessor initialized.Partition:");
            //_signalRController.SendMessage("c#", "SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            Logger.LogInformation("SimpleEventProcessor initialized.Partition");
            //Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Logger.LogInformation("Error on Partition");
            Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                Logger.LogInformation("#######################Message received.");
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                _hubContext.Clients.All.SendAsync("sendToAll", "signalREventProcessor", "Message received.");
                

                //_signalRController.SendMessage("c#", "Message received.");

                //Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{data}'");
            }

            return context.CheckpointAsync();
        }

    }
}

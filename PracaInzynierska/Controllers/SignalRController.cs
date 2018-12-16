using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PracaInzynierska.Hubs;
using PracaInzynierska.Others;

namespace PracaInzynierska.Controllers
{
    public class SignalRController : ISignalR
    {
        private static IHubContext<IoTSignalRHub> _hubContext;

        public SignalRController(IHubContext<IoTSignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void SendMessage(string name, string message)
        {
            _hubContext.Clients.All.SendAsync("sendToAll", name, message);
        }
    }
}

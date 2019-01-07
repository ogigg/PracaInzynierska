using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PracaInzynierska.Models
{
    public class DeviceDataMessage
    {
        public string DeviceId { get; set; }

        public string MessageType { get; set; } //data

        public string Message { get; set; }

        public DateTime Time { get; set; }

        public ICollection<PortStatus> PortStatus{ get; set; }
        public DeviceDataMessage()
        {
            PortStatus = new Collection<PortStatus>();
        }
    }

    public class PortStatus
    {
        public int Id { get; set; }
        public string ValueType { get; set; }
        public string Value { get; set; }
    }
}

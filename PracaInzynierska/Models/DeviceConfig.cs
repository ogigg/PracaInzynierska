using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PracaInzynierska.Models
{
    public class DeviceConfig
    {
        public string DeviceId { get; set; }

        public string MessageType { get; set; } //configuration

        public string DeviceName { get; set; }

        public string Message { get; set; }

        public DateTime Time { get; set; }

        public ICollection<PortAttributes> PortAttributes { get; set; }

        public DeviceConfig()
        {
            PortAttributes = new Collection<PortAttributes>();
        }
    }
}

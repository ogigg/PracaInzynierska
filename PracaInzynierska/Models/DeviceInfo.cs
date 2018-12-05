using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PracaInzynierska.Models
{
    public class DeviceInfo
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }

        public string DeviceName { get; set; }

        public string Message { get; set; }

        public ICollection<PortAttributes> PortAttributes { get; set; }

        public DeviceInfo()
        {
            PortAttributes = new Collection<PortAttributes>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaInzynierska.Models
{
    public class PortAttributes
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string ValueType { get; set; }
        public string Unit { get; set; }
        public float MaxValue { get; set; }
        public float MinValue { get; set; }
        public string GPIOType { get; set; }
        public Value Value{ get; set; }
    }
}

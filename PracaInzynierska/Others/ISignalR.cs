using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaInzynierska.Others
{

    public interface ISignalR
    {
        void SendMessage(string name, string message);
    }
}

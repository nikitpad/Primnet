using System;
using System.Net.Sockets;

namespace Primnet
{
    public class ConnectedEventArgs : EventArgs
    {
        public ConnectedEventArgs(Socket s)
        {
            Client = s;
        }
        public Socket Client { get; private set; }
    }
}

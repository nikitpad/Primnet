using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Primnet
{
    public class PrimnetServer : IDisposable
    {
        /// <summary>
        /// The port our server is running on.
        /// </summary>
        public ushort Port { get; private set; }
        /// <summary>
        /// The default maximum amount of data that can be recieved from a client in one go.
        /// </summary>
        public int DefaultRecieveBufferSize { get; set; } = 1024;
        public List<PrimnetClient> Clients { get; private set; } = new List<PrimnetClient>();
        /// <summary>
        /// The IP Address our server is running on.
        /// </summary>
        public IPAddress Address { get; private set; }
        /// <summary>
        /// Indicating if the server is currently running or not.
        /// </summary>
        public bool Running { get; private set; }
        public TcpListener BaseListener => _listener; private TcpListener _listener;
        public event EventHandler<ConnectedEventArgs> OnConnected;
        public event EventHandler<DataEventArgs> OnDataFromClient; // Sender object is the client
        public PrimnetServer(ushort port, IPAddress addr)
        {
            Running = true;
            Address = addr ?? IPAddress.Any;
            Port = port;
            _listener = new TcpListener(Address, Port);
            _listener.Start();
        }
        /// <summary>
        /// Start listening for an incoming connection.
        /// </summary>
        public void StartListening() => _listener.BeginAcceptSocket(new AsyncCallback(_onConnect), this);
        private void _onConnect(IAsyncResult result) 
        {
            try
            {
                Socket s = BaseListener.EndAcceptSocket(result); // Get the client socket object when it's ready
                OnConnected?.Invoke(this, new ConnectedEventArgs(s)); // Trigger the OnConnected event
                var client = new PrimnetClient(s, this) { RecieveBufferSize = DefaultRecieveBufferSize }; // Initialize a client object from the accepted socket
                client.StartProcessing(); // Start waiting for data from the client
                client.OnDataAvaliable += (s_, e) => OnDataFromClient?.Invoke(s_, e); 
                Clients.Add(client); // Add the client object of the connection to the client list
                StartListening(); // Start waiting for a new connection after this one has been established
            }
            catch(Exception e) { Console.WriteLine(e); }
        }
        /// <summary>
        /// Disposes the current PrimnetServer object.
        /// </summary>
        public void Dispose()
        {
            Clients.ForEach(x => x.Kill()); // Kill all existing connections
            Clients.Clear(); // Clear the client list
            BaseListener.Stop(); // Stop listening for connections
            Running = false; 
        }
    }
}

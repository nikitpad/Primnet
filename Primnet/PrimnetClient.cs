using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Primnet
{
    public class PrimnetClient : IDisposable
    {
        public PrimnetServer Server => _server; private PrimnetServer _server;
        /// <summary>
        /// The raw socket of the client.
        /// </summary>
        public Socket BaseSocket => _sock;      private Socket _sock;
        /// <summary>
        /// The IP Address and the port of the client
        /// </summary>
        public IPEndPoint EndPoint => _ep;      private IPEndPoint _ep;
        private byte[] _buffer;
        /// <summary>
        /// The maximum number of bytes to recieve during a go.
        /// </summary>
        public int RecieveBufferSize
        {
            get
            {
                return _sock.ReceiveBufferSize;
            }
            set
            {
                _buffer = new byte[value];
                _sock.ReceiveBufferSize = value;
            }
        }
        /// <summary>
        /// A delegate to encapsulate an event with no parameters.
        /// </summary>
        public delegate void EmptyEvent();
        /// <summary>
        /// An event that's triggered when some data was sent to the server by the client.
        /// </summary>
        public event EventHandler<DataEventArgs> OnDataAvaliable;
        /// <summary>
        /// An event that's triggered when the connection of the server to the client was destroyed.
        /// </summary>
        public event EmptyEvent OnDead;
        public bool IsAlive { get; private set; } = true;
        public PrimnetClient(Socket s, PrimnetServer serv)
        {
            _sock = s;
            _server = serv;
            _ep = s.RemoteEndPoint as IPEndPoint;
        }  
        public void StartProcessing()               => _sock.BeginReceive(_buffer, 0, RecieveBufferSize, 0, new AsyncCallback(_endRecieve), this);
        /// <summary>
        /// Sends an array of bytes to the client.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public void Send(byte[] data)               => _sock.BeginSend(data, 0, data.Length, 0, new AsyncCallback(_endSend), this);
        /// <summary>
        /// Sends a string to the client.
        /// </summary>
        /// <param name="data">The string</param>
        /// <param name="enc">The encoding to use for the string.</param>
        public void Send(string data, Encoding enc) => _sock.BeginSend(enc.GetBytes(data), 0, data.Length, 0, new AsyncCallback(_endSend), this);
        /// <summary>
        /// Sends a UTF8-encoded string to the client.
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)               => _sock.BeginSend(Encoding.UTF8.GetBytes(data), 0, data.Length, 0, new AsyncCallback(_endSend), this);
        /// <summary>
        /// Sends a single byte to the client.
        /// </summary>
        /// <param name="data">The byte to be sent.</param>
        public void Send(byte data)                 => _sock.BeginSend(new byte[] { data }, 0, 1, 0, new AsyncCallback(_endSend), this);
        /// <summary>
        /// Reads all bytes from a file and then sends them to the client.
        /// </summary>
        /// <param name="filename">The path to the file to read from.</param>
        public void SendFileData(string filename)   => _sock.BeginSend(File.ReadAllBytes(filename), 0, File.ReadAllBytes(filename).Length, 0, new AsyncCallback(_endSend), this);
        /// <summary>
        /// Stop the connection of the server with the client.
        /// </summary>
        public void Kill()
        {
            IsAlive = false;
            _sock.Disconnect(false);
            _server.Clients.Remove(this);
        }
        /// <summary>
        /// Dispose the current PrimnetClient object.
        /// </summary>
        public void Dispose()
        {
            Kill();
            OnDead?.Invoke();
            _sock.Dispose();
        }
        private void _endRecieve(IAsyncResult result)
        {
            int recieved = 0;
            try
            {
                recieved = _sock.EndReceive(result); // Gets the number of bytes recieved from the socket once it is avaliable, the actual data is in _buffer.
            }
            catch (SocketException) { Dispose(); }
            if (recieved > 0)
                    OnDataAvaliable?.Invoke(this, new DataEventArgs(_buffer.Take(recieved).ToArray())); // Trigger the OnDataAvaliable event
            _sock.BeginReceive(_buffer, 0, RecieveBufferSize, 0, new AsyncCallback(_endRecieve), this); // Start waiting for the next portion of data
        }
        private void _endSend(IAsyncResult result)
        {
            try
            {
                _sock.EndSend(result);
            }
            catch(SocketException) { Dispose(); }
        }
    }
}

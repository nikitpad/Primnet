using System;

namespace Primnet
{
    public class DataEventArgs : EventArgs
    {
        public DataEventArgs(byte[] data)
        {
            Data = data;
        }
        public byte[] Data { get; private set; }
    }
}

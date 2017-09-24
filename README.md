# Primnet
A simple and lightweight TCP server library.
Example usage:
```cs
using System;
using System.Text;
using Primnet;

namespace PrimnetTestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            PrimnetServer server = new PrimnetServer(30522, null);
            server.StartListening();
            server.OnDataFromClient += (s, e) =>
            {
                Console.WriteLine($"We got the following data from {(s as PrimnetClient).EndPoint}:\r\n{Encoding.UTF8.GetString(e.Data)}\r\nSending back response...");
                (s as PrimnetClient).Send("hello!");
            };
            Console.ReadKey(false);
        }
    }
}

```
[Now avaliable on NuGet](https://www.nuget.org/packages/nikitpad.library.Primnet/1.0.0)

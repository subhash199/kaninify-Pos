using DataHandlerLibrary.Interfaces;
using System.Net.Sockets;

namespace DataHandlerLibrary.Models.Printer
{
    internal sealed class EthernetPrinterTransport : IPrinterTransport
    {
        private readonly string _ipAddress;
        private readonly int _port;

        public EthernetPrinterTransport(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public void Send(byte[] bytes)
        {
            using var client = new TcpClient();
            client.SendTimeout = 5000;
            client.ReceiveTimeout = 5000;
            client.Connect(_ipAddress, _port);
            using var stream = client.GetStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
    }
}

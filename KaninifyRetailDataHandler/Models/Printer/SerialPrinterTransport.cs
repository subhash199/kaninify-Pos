using DataHandlerLibrary.Interfaces;
using System.IO.Ports;

namespace DataHandlerLibrary.Models.Printer
{
    internal sealed class SerialPrinterTransport : IPrinterTransport
    {
        private readonly string _portName;
        private readonly int _baudRate;

        public SerialPrinterTransport(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public void Send(byte[] bytes)
        {
            using var port = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };

            port.Open();
            port.Write(bytes, 0, bytes.Length);
            port.BaseStream.Flush();
            port.Close();
        }
    }
}

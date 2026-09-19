using DataHandlerLibrary.Interfaces;
using System.Text;

namespace DataHandlerLibrary.Models.Printer
{
    internal sealed class EscPosDirectPrintDevice : IPrintDevice
    {
        private static readonly byte[] Init = { 0x1B, 0x40 };
        private static readonly byte[] AlignLeftCmd = { 0x1B, 0x61, 0x00 };
        private static readonly byte[] AlignCenterCmd = { 0x1B, 0x61, 0x01 };
        private static readonly byte[] BoldOn = { 0x1B, 0x45, 0x01 };
        private static readonly byte[] BoldOff = { 0x1B, 0x45, 0x00 };
        private static readonly byte[] Cut = { 0x1D, 0x56, 0x42, 0x03 };
        private static readonly byte[] BarcodeWidth = { 0x1D, 0x77, 0x02 };
        private static readonly byte[] BarcodeHeight = { 0x1D, 0x68, 0x50 };
        private static readonly byte[] BarcodeTextNone = { 0x1D, 0x48, 0x00 };

        private readonly IPrinterTransport _transport;
        private readonly int _maxChars;
        private readonly MemoryStream _buffer = new MemoryStream();
        private readonly Encoding _encoding = Encoding.GetEncoding(860);

        public EscPosDirectPrintDevice(IPrinterTransport transport, int maxChars)
        {
            _transport = transport;
            _maxChars = maxChars;
        }

        public void InitializePrint()
        {
            Clear();
            Write(Init);
            Write(AlignLeftCmd);
        }

        public void AlignCenter() => Write(AlignCenterCmd);
        public void AlignLeft() => Write(AlignLeftCmd);

        public void Append(string text)
        {
            if (text == null)
            {
                return;
            }

            var bytes = _encoding.GetBytes(text);
            Write(bytes);
            NewLine();
        }

        public void NewLine() => Write(new byte[] { 0x0A });

        public void Separator()
        {
            var line = new string('-', Math.Max(1, _maxChars));
            Append(line);
        }

        public void BoldMode(string text)
        {
            Write(BoldOn);
            Append(text);
            Write(BoldOff);
        }

        public void Code128(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var payload = _encoding.GetBytes("{B" + text.Trim().ToUpperInvariant());
            Write(BarcodeWidth);
            Write(BarcodeHeight);
            Write(BarcodeTextNone);
            Write(new byte[] { 0x1D, 0x6B, 0x49, (byte)payload.Length });
            Write(payload);
            NewLine();
        }

        public void FullPaperCut() => Write(Cut);

        public void PrintDocument()
        {
            var bytes = _buffer.ToArray();
            if (bytes.Length == 0)
            {
                return;
            }

            _transport.Send(bytes);
            Clear();
        }

        public void Clear()
        {
            _buffer.SetLength(0);
            _buffer.Position = 0;
        }

        private void Write(byte[] bytes)
        {
            _buffer.Write(bytes, 0, bytes.Length);
        }
    }
}

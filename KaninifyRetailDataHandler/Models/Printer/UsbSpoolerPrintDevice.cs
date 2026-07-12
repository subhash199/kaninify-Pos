using DataHandlerLibrary.Interfaces;

namespace DataHandlerLibrary.Models.Printer
{
    internal sealed class UsbSpoolerPrintDevice : IPrintDevice
    {
        private readonly ESC_POS_USB_NET.Printer.Printer _inner;

        public UsbSpoolerPrintDevice(ESC_POS_USB_NET.Printer.Printer inner)
        {
            _inner = inner;
        }

        public void InitializePrint() => _inner.InitializePrint();
        public void AlignCenter() => _inner.AlignCenter();
        public void AlignLeft() => _inner.AlignLeft();
        public void Append(string text) => _inner.Append(text);
        public void NewLine() => _inner.NewLine();
        public void Separator() => _inner.Separator();
        public void BoldMode(string text) => _inner.BoldMode(text);
        public void FullPaperCut() => _inner.FullPaperCut();
        public void PrintDocument() => _inner.PrintDocument();
        public void Clear() => _inner.Clear();
    }
}

namespace DataHandlerLibrary.Interfaces
{
    internal interface IPrinterTransport
    {
        void Send(byte[] bytes);
    }
}

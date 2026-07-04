namespace DataHandlerLibrary.Interfaces
{
    internal interface IPrintDevice
    {
        void InitializePrint();
        void AlignCenter();
        void AlignLeft();
        void Append(string text);
        void NewLine();
        void Separator();
        void BoldMode(string text);
        void FullPaperCut();
        void PrintDocument();
        void Clear();
    }
}

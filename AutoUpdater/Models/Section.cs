namespace AutoUpdater.Models;

public struct Section
{
    public int Index;
    public string Name;
    public string VirtualSize;
    public string VirtualAddress;
    public string SizeOfRawData;
    public string FilePointerToRawData;
    public string FilePointerToRelocationTable;
    public string FilePointerToLineNumbers;
}
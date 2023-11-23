using AutoUpdate.Resolvers;

namespace AutoUpdate.Models;

public class Config
{
    public string? WorkingDirectory;
    public string? PdbFile;
    public string? ExeFile;
    public string? LlvmInstallDirectory;
    public string? PdbUtil;
    public string? DemangleUtil;
    public string? OutputFile = "output.txt";

    public List<UpdateItem> UpdateItems = new();
    
}
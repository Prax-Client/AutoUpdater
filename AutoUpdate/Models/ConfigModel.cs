using AutoUpdate.Resolvers;

namespace AutoUpdate.Models;

public class ConfigModel
{
    public string? WorkingDirectory;
    public string? PdbFile;
    public string? ExeFile;
    public string? LlvmInstallDirectory;
    public string? PdbUtil;
    public string? DemangleUtil;

    public List<UpdateItem> UpdateItems = new();
    
}
using AutoUpdater.Resolvers;

namespace AutoUpdater.Configs;

public class Preferences
{
    public string? WorkingDirectory;
    public string? PdbFile;
    public string? ExeFile;
    public string? LlvmInstallDirectory;
    public string? PdbUtil;
    public string? DemangleUtil;
    public string? OutputFile = "output.txt";
}

public class UpdateConfig
{
    public string? Author;
    public List<UpdateItem> Items = new();
}

public class Config
{
    public Preferences? Preferences = new();
    public UpdateConfig? SDK = new();
}

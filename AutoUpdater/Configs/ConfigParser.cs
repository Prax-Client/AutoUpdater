using AutoUpdater.Resolvers;
using AutoUpdater.Resolvers.Impl;
using Newtonsoft.Json;

namespace AutoUpdater.Configs;

public static class ConfigParser
{
    public static Config LoadConfig()
    {
        // Load json file
        var json = File.ReadAllText(Environment.CurrentDirectory + "\\config.json");
        // Deserialize json
        var config = JsonConvert.DeserializeObject<Config>(json);
        // Return config
        
        // Make sure to convert all the resolvers to the correct type
        foreach (var updateItem in config!.UpdateItems)
        {
            updateItem.Resolver = updateItem.Resolver.Type switch
            {
                ResolverType.Offset => new OffsetResolver(updateItem.Resolver.Value),
                ResolverType.Pattern => new PatternResolver(updateItem.Resolver.Value),
                _ => updateItem.Resolver
            };
        }
        
        return config;
    }

    public static void SaveConfig()
    {
        // Serialize config
        var json = JsonConvert.SerializeObject(Program.Config, Formatting.Indented);
        // Write to file
        File.WriteAllText(Environment.CurrentDirectory + "\\config.json", json);
    }

    public static bool IsConfigValid()
    {
        // Make sure the config file exists
        if (!File.Exists(Environment.CurrentDirectory + "\\config.json"))
        {
            return false;
        }
        
        // Make sure its not empty
        if (new FileInfo(Environment.CurrentDirectory + "\\config.json").Length == 0)
        {
            return false;
        }
        
        // Make sure the config is valid
        try
        {
            LoadConfig();
        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
    
    public static void SetupConfig()
    {
        Console.Write("Enter the directory of LLVM's installation: ");
        Program.Config.LlvmInstallDirectory = Console.ReadLine();
        
        Program.Config.PdbUtil = Program.Config.LlvmInstallDirectory + "\\llvm-pdbutil.exe";
        Program.Config.DemangleUtil = Program.Config.LlvmInstallDirectory + "\\llvm-undname.exe";
        
        if (!File.Exists(Program.Config.PdbUtil))
        {
            Console.WriteLine("llvm-pdbutil.exe not found. Please enter the directory of llvm-pdbutil.exe:");
            Program.Config.PdbUtil = Console.ReadLine();
        }
        
        if (!File.Exists(Program.Config.DemangleUtil))
        {
            Console.WriteLine("llvm-undname.exe not found. Please enter the directory of llvm-undname.exe:");
            Console.WriteLine("This file is usually installed when using MSYS2. If you do not have this file, you can install MSYS2 from https://www.msys2.org/");
            Program.Config.DemangleUtil = Console.ReadLine();
        }
        
        Console.Write("Enter the directory you wish to download the server to: ");
        Program.Config.WorkingDirectory = Console.ReadLine();
        
        // Create a new update item for an example
        UpdateItem example = new()
        {
            Name = "Example Offset",
            Function = "?getSupplies@Player@@QEBAAEBVPlayerInventory@@XZ",
            Resolver = new OffsetResolver("3")
        };
        
        // Add the example update item to the config
        Program.Config.UpdateItems.Add(example);
        
        // Add pattern resolver example
        UpdateItem example2 = new()
        {
            Name = "Example Pattern",
            Function = "?getSupplies@Player@@QEBAAEBVPlayerInventory@@XZ",
            Resolver = new PatternResolver("? ? ? ? C3")
        };
        
        // Add the example update item to the config
        Program.Config.UpdateItems.Add(example2);
        
        SaveConfig();
    }
}
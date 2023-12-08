using AutoUpdater.Resolvers;
using AutoUpdater.Resolvers.Impl;
using Newtonsoft.Json;

namespace AutoUpdater.Configs;

public static class ConfigParser
{
    public static Config LoadConfigs()
    {
        // Check is valid
        if (!IsDumpValid() || !ArePreferencesValid())
        {
            throw new Exception("Config is not valid");
        }

        Config config = new();
        
        
        
        // Load json file
        var prefJson = File.ReadAllText(Environment.CurrentDirectory + "\\preferences.json");
        // Deserialize json
        config.Preferences = JsonConvert.DeserializeObject<Preferences>(prefJson);
        
        // Load json file
        var dumpJson = File.ReadAllText(Environment.CurrentDirectory + "\\dump.json");
        // Deserialize json
        config.SDK = JsonConvert.DeserializeObject<UpdateConfig>(dumpJson);
        
        // Make sure to convert all the resolvers to the correct type
        foreach (var updateItem in config!.SDK.Items)
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
        var prefJson = JsonConvert.SerializeObject(Program.Config.Preferences, Formatting.Indented);
        // Write to file
        File.WriteAllText(Environment.CurrentDirectory + "\\preferences.json", prefJson);
        
        // Serialize config
        var dumpJson = JsonConvert.SerializeObject(Program.Config.SDK, Formatting.Indented);
        // Write to file
        File.WriteAllText(Environment.CurrentDirectory + "\\dump.json", dumpJson);
    }

    public static bool ArePreferencesValid()
    {
        // Make sure the config file exists
        return File.Exists(Environment.CurrentDirectory + "\\dump.json") && File.Exists(Environment.CurrentDirectory + "\\preferences.json");
    }

    public static bool IsDumpValid()
    {
        // Make sure its not empty
        return new FileInfo(Environment.CurrentDirectory + "\\dump.json").Length != 0 && new FileInfo(Environment.CurrentDirectory + "\\preferences.json").Length != 0;
    }
    
    public static void SetupConfig()
    {
        
        if (!File.Exists("preferences.json") || File.ReadAllText("preferences.json") == "")
        {
            Console.Write("Enter the directory of LLVM's installation: ");
            Program.Config.Preferences!.LlvmInstallDirectory = Console.ReadLine();
        
            Program.Config.Preferences.PdbUtil = Program.Config.Preferences.LlvmInstallDirectory + "\\llvm-pdbutil.exe";
            Program.Config.Preferences.DemangleUtil = Program.Config.Preferences.LlvmInstallDirectory + "\\llvm-undname.exe";
        
            if (!File.Exists(Program.Config.Preferences.PdbUtil))
            {
                Console.WriteLine("llvm-pdbutil.exe not found. Please enter the directory of llvm-pdbutil.exe:");
                Program.Config.Preferences.PdbUtil = Console.ReadLine();
            }
        
            if (!File.Exists(Program.Config.Preferences.DemangleUtil))
            {
                Console.WriteLine("llvm-undname.exe not found. Please enter the directory of llvm-undname.exe:");
                Console.WriteLine("This file is usually installed when using MSYS2. If you do not have this file, you can install MSYS2 from https://www.msys2.org/");
                Program.Config.Preferences.DemangleUtil = Console.ReadLine();
            }
        
            Console.Write("Enter the directory you wish to download the server to: ");
            Program.Config.Preferences.WorkingDirectory = Console.ReadLine();
        }
        
        if (!File.Exists("dump.json") || File.ReadAllText("dump.json") == "")
        {
            
        
            // Create a new update item for an example
            UpdateItem example = new()
            {
                Name = "Example Offset",
                Function = "?getSupplies@Player@@QEBAAEBVPlayerInventory@@XZ",
                Resolver = new OffsetResolver("3")
            };
        
            // Add the example update item to the config
            Program.Config.SDK.Items.Add(example);
        
            // Add pattern resolver example
            UpdateItem example2 = new()
            {
                Name = "Example Pattern",
                Function = "?getSupplies@Player@@QEBAAEBVPlayerInventory@@XZ",
                Resolver = new PatternResolver("? ? ? ? C3")
            };
        
            // Add the example update item to the config
            Program.Config.SDK.Items.Add(example2);
        }
        
        SaveConfig();
    }
}
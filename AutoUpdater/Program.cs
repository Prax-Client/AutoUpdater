using AutoUpdater.Configs;
using AutoUpdater.Models;
using AutoUpdater.Utility;

namespace AutoUpdater;

internal static class Program
{
    public static Config Config = new();

    public static Dictionary<string, PublicSymbol> PublicSymbolsDict = new();
    public static List<Section> Sections = new();

    private static async Task Main()
    {
        Console.Title = "Auto Updater";
        Console.ForegroundColor = ConsoleColor.DarkCyan;

        Misc.DrawASCII();

        if (!ConfigParser.ArePreferencesValid())
        {
            // Delete it if it exists
            if (File.Exists(Environment.CurrentDirectory + "\\preferences.json"))
            {
                File.Delete(Environment.CurrentDirectory + "\\preferences.json");
            }
            Console.WriteLine("Configuration file not found. Creating");
            
            
            ConfigParser.SetupConfig();
            return;
        }

        if (!ConfigParser.IsDumpValid())
        {
            if (File.Exists(Environment.CurrentDirectory + "\\dump.json"))
            {
                File.Delete(Environment.CurrentDirectory + "\\dump.json");
            }
            Console.WriteLine("Configuration file not found. Creating");
            
            ConfigParser.SetupConfig();
            return;
        }

        
        Console.WriteLine("Loading config...");
        Config = ConfigParser.LoadConfigs();
        
        Console.WriteLine("Download latest version? (Y/n)");
        var input = Console.ReadLine();

        if (!input!.ToLower().Contains('n'))
        {
            // Try to print out the version 
            try
            {
                var folderName = await Misc.DownloadLatest();
                Console.WriteLine($"Version: {folderName}");

                Config.Preferences.PdbFile = $@"{Config.Preferences.WorkingDirectory}\{folderName}\bedrock_server.pdb";
                Config.Preferences.ExeFile = $@"{Config.Preferences.WorkingDirectory}\{folderName}\bedrock_server.exe";

                ConfigParser.SaveConfig();
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get version.");
            }
        }
        else
        {
            Console.Write("Enter a folder name (Must be in the working directory):");
            var folderName = Console.ReadLine();

            Config.Preferences.PdbFile = $@"{Config.Preferences.WorkingDirectory}\{folderName}\bedrock_server.pdb";
            Config.Preferences.ExeFile = $@"{Config.Preferences.WorkingDirectory}\{folderName}\bedrock_server.exe";

            ConfigParser.SaveConfig();
        }

        Console.Clear();
        Misc.DrawASCII();

        PublicSymbolsDict = LLVM.DumpPublics();
        Sections = LLVM.DumpSections();

        var offsets = Update.UpdateFromConfig(Config);


        Console.Clear();
        Misc.DrawASCII();

        Console.WriteLine("==========================================");

        Console.WriteLine("Offsets:");
        foreach (var offset in offsets)
        {
            Console.WriteLine($"{offset.Key}: 0x{offset.Value:X}");
        }

        Console.WriteLine("==========================================");

        // Export to file
        Console.WriteLine("Exporting to file...");

        if (Config.Preferences.OutputFile != null)
            await File.WriteAllTextAsync(Config.Preferences.OutputFile,
                string.Join("\n", offsets.Select(x => $"{x.Key}=0x{x.Value:X}")));

        Console.WriteLine("Done. Press enter to start again.");

        Console.ReadLine();
        Console.Clear();
    }
}
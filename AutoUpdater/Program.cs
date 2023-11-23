using AutoUpdater.Models;
using AutoUpdater.Utility;

namespace AutoUpdater 
{
    internal class Program
    {
        public static Models.Config Config = new Models.Config();
        
        public static Dictionary<string, PublicSymbol> PublicSymbolsDict = new Dictionary<string, PublicSymbol>();
        public static List<Section> Sections = new List<Section>();
        
        static void Main()
        {
            Console.Title = "Auto Updater";
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            
            Console.WriteLine("""
                                  \          |            |  |          |         |              
                                 _ \   |  |   _|   _ \    |  | _ \   _` |   _` |   _|   -_)   _| 
                               _/  _\ \_,_| \__| \___/   \__/ .__/ \__,_| \__,_| \__| \___| _|   
                                                             _|                                  
                              """);
            
            
            if (!ConfigParser.IsConfigValid())
            {
                // Delete it if it exists
                if (File.Exists(Environment.CurrentDirectory + "\\config.json"))
                {
                    File.Delete(Environment.CurrentDirectory + "\\config.json");
                }
                Console.WriteLine("Creating config...");
                ConfigParser.SetupConfig();
            } else
            {
                Console.WriteLine("Loading config...");
                Config = ConfigParser.LoadConfig();
            }
            
            Console.WriteLine("Download latest version? (Y/n)");
            string? input = Console.ReadLine();

            if (input!.ToLower().Contains("n"))
            {
                
                // Try to print out the version 
                try
                {
                    string folderName = Misc.DownloadLatest().Result;
                    Console.WriteLine($"Version: {folderName}");
                
                    Config.PdbFile = $"{Config.WorkingDirectory}\\{folderName}\\bedrock_server.pdb";
                    Config.ExeFile = $"{Config.WorkingDirectory}\\{folderName}\\bedrock_server.exe";
                
                    ConfigParser.SaveConfig();
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to get version.");
                }
            }
            else
            {
                Console.Write("Enter folder name (Must be in the working directory):");
                string? folderName = Console.ReadLine();
                
                Config.PdbFile = $"{Config.WorkingDirectory}\\{folderName}\\bedrock_server.pdb";
                Config.ExeFile = $"{Config.WorkingDirectory}\\{folderName}\\bedrock_server.exe";
                
                ConfigParser.SaveConfig();
            }
            
            
            PublicSymbolsDict = LLVM.DumpPublics();
            Sections = LLVM.DumpSections();

            Dictionary<string, int> offsets = Update.UpdateFromConfig(Config);
            
            Console.WriteLine("==========================================");
            
            Console.WriteLine("Offsets:");
            foreach (var offset in offsets)
            {
                Console.WriteLine($"{offset.Key}: 0x{offset.Value.ToString("X")}");
            }
            
            Console.WriteLine("==========================================");
            
            // Export to file
            Console.WriteLine("Exporting to file...");
            if (Config.OutputFile != null)
                File.WriteAllText(Config.OutputFile,
                    string.Join("\n", offsets.Select(x => $"{x.Key}=0x{x.Value.ToString("X")}")));

            Console.WriteLine("Done.");

            Console.ReadLine();
            Console.Clear();
            
            Main();
        }
    }
}
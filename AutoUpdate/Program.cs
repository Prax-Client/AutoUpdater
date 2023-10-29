using AutoUpdate.Models;
using AutoUpdate.Resolvers;
using AutoUpdate.Resolvers.Impl;
using AutoUpdate.Structs;
using AutoUpdate.Utility;

namespace AutoUpdate 
{
    internal class Program
    {
        public static ConfigModel Config = new ConfigModel();
        
        public static Dictionary<string, PublicSymbol> PublicSymbolsDict = new Dictionary<string, PublicSymbol>();
        public static List<Section> Sections = new List<Section>();
        
        static void Main()
        {
            Console.Title = "Auto Updater";
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            
            Console.WriteLine("""
                                _
                               /_/  _/__   / /_   _/_ _/__  _
                              / //_// /_/ /_//_//_//_|/ /_'/
                                            /
                              """);
            
            if (!ConfigParser.IsConfigValid())
            {
                // Delete it if it exists
                if (File.Exists(Environment.CurrentDirectory + "\\config.json"))
                {
                    File.Delete(Environment.CurrentDirectory + "\\config.json");
                }
                Console.WriteLine("Config file not found. Creating one...");
                ConfigParser.SetupConfig();
            } else
            {
                Console.WriteLine("Config file found. Loading...");
                Config = ConfigParser.LoadConfig();
            }

            // Try to print out the version 
            try
            {
                Console.WriteLine($"Version: {Misc.GetVersion()}");
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to get version.");
            }
            
            
            PublicSymbolsDict = LLVM.DumpPublics();
            Sections = LLVM.DumpSections();

            Update.UpdateFromConfig(Config);
            
            Console.ReadLine();
            Console.Clear();
            
            Main();
        }
    }
}
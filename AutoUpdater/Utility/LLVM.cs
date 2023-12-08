using System.Text.RegularExpressions;
using AutoUpdater.Models;

namespace AutoUpdater.Utility;

public static class LLVM
{
    public static Dictionary<string, PublicSymbol> DumpPublics()
    {
        Console.WriteLine("Dumping public symbols...");
        Misc.RunProcess(Program.Config.Preferences.PdbUtil, 
            $"dump --publics \"{Program.Config.Preferences.PdbFile}\" > \"{Program.Config.Preferences.WorkingDirectory}\\publics.txt\"");
        
        const string pattern = @"(\d+) \| (\S+) \[size = (\d+)\] `([^`]+)`\s+flags = ([^,]+), addr = ([\d:]+)";

        var publicsMatches = Regex.Matches(File.ReadAllText($"{Program.Config.Preferences.WorkingDirectory}\\publics.txt"), pattern, RegexOptions.Multiline);

        var publicSymbolsDict = new Dictionary<string, PublicSymbol>();
            
        foreach (Match match in publicsMatches)
        {
            var symbol = match.Groups[4].Value;
                
            publicSymbolsDict.Add(symbol, new PublicSymbol
            {
                Number = match.Groups[1].Value,
                Name = symbol,
                Size = int.Parse(match.Groups[3].Value),
                Address = int.Parse(match.Groups[6].Value.Split(":")[1]),
                SectionIndex = int.Parse(match.Groups[6].Value.Split(":")[0]),
            });
        }
        
        return publicSymbolsDict;
    }

    public static List<Section> DumpSections()
    {
        Console.WriteLine("Dumping sections...");
        Misc.RunProcess(Program.Config.Preferences.PdbUtil, $"dump --section-headers \"{Program.Config.Preferences.PdbFile}\" > \"{Program.Config.Preferences.WorkingDirectory}\\sections.txt\"");

        var sections = new List<Section>();
            
        // Get the sections
        var sectionsInput = File.ReadAllText($"{Program.Config.Preferences.WorkingDirectory}\\sections.txt");
            
        // Define a regular expression pattern to match section headers and their properties
        var sectionPattern = @"SECTION HEADER #\d+([\s\S]*?)(?=SECTION HEADER #|\z)";            // Create a regular expression object
        var regex = new Regex(sectionPattern, RegexOptions.Multiline);

        // Find all matches in the file content
        var sectionMatches = regex.Matches(sectionsInput);

        // Log the information for each section header
        foreach (Match match in sectionMatches)
        {
            var sectionText = match.Value;
                
            sections.Add(new Section
            {
                Index = int.Parse(ExtractValue(sectionText, @"SECTION HEADER #(\d+)")),
                Name = ExtractValue(sectionText, @"^\s+([\w.]+) name"),
                VirtualSize = ExtractValue(sectionText, @"\s+(\w+) virtual size"),
                VirtualAddress = ExtractValue(sectionText, @"\s+(\w+) virtual address"),
                SizeOfRawData = ExtractValue(sectionText, @"\s+(\w+) size of raw data"),
                FilePointerToRawData = ExtractValue(sectionText, @"\s+(\w+) file pointer to raw data"),
                FilePointerToRelocationTable = ExtractValue(sectionText, @"\s+(\w+) file pointer to relocation table"),
                FilePointerToLineNumbers = ExtractValue(sectionText, @"\s+(\w+) file pointer to line numbers")
            });
                
        }
        
        return sections;
    }
    
    public static string Demangle(string input)
    {
        try
        {
            return Misc.RunProcessAndGetOutput(Program.Config.Preferences.DemangleUtil, input).Split("\n")[5];
        }
        catch (Exception)
        {
            return input;
        }
    }

    private static string ExtractValue(string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }
    
    
    public static Dictionary<ulong, string> GetSymbolsForSymbolResolver()
    {
        var symbolDict = new Dictionary<ulong, string>();
            
        foreach (var symbol in Program.PublicSymbolsDict)
        {
            // Make sure it doesn't already exist
            if (!symbolDict.ContainsKey((ulong) symbol.Value.Address))
                symbolDict.Add((ulong) symbol.Value.Address, symbol.Key);
            
        }

        return symbolDict;
    }
}
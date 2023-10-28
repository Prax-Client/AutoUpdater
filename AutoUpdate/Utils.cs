using System.Diagnostics;
using System.Text.RegularExpressions;
using AutoUpdate.Structs;
using Iced.Intel;

namespace AutoUpdate;

public class Utils
{
    public static string RunProcess(string filename, string arguments, bool writeOutput = false)
    {
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = writeOutput;
        cmd.StartInfo.CreateNoWindow = false;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine($"\"{filename}\" {arguments}");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        if (writeOutput)
            cmd.WaitForExit();
            
        return writeOutput ? cmd.StandardOutput.ReadToEnd() : "";

    }

    public static Dictionary<string, PublicSymbol> DumpPublics(string pdbutil, string inputFile)
    {
        RunProcess(pdbutil, $"dump --publics \"{inputFile}\" > publics.txt", true);
        
        string pattern = @"(\d+) \| (\S+) \[size = (\d+)\] `([^`]+)`\s+flags = ([^,]+), addr = ([\d:]+)";

        MatchCollection publicsMatches = Regex.Matches(File.ReadAllText("publics.txt"), pattern, RegexOptions.Multiline);

        Dictionary<string, PublicSymbol> publicSymbolsDict = new Dictionary<string, PublicSymbol>();
            
        foreach (Match match in publicsMatches)
        {
            string symbol = match.Groups[4].Value;
                
            publicSymbolsDict.Add(symbol, new PublicSymbol()
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

    public static List<Section> DumpSections(string pdbutil, string inputFile )
    {
        RunProcess(pdbutil, $"dump --section-headers \"{inputFile}\" > sections.txt", true);

        List<Section> sections = new List<Section>();
            
        // Get the sections
        string sectionsInput = File.ReadAllText("sections.txt");
            
        // Define a regular expression pattern to match section headers and their properties
        string sectionPattern = @"SECTION HEADER #\d+([\s\S]*?)(?=SECTION HEADER #|\z)";            // Create a regular expression object
        Regex regex = new Regex(sectionPattern, RegexOptions.Multiline);

        // Find all matches in the file content
        MatchCollection sectionMatches = regex.Matches(sectionsInput);

        // Log the information for each section header
        foreach (Match match in sectionMatches)
        {
            string sectionText = match.Value;
                
            sections.Add(new Section()
            {
                Index = int.Parse(Utils.ExtractValue(sectionText, @"SECTION HEADER #(\d+)")),
                Name = Utils.ExtractValue(sectionText, @"^\s+([\w.]+) name"),
                VirtualSize = Utils.ExtractValue(sectionText, @"\s+(\w+) virtual size"),
                VirtualAddress = Utils.ExtractValue(sectionText, @"\s+(\w+) virtual address"),
                SizeOfRawData = Utils.ExtractValue(sectionText, @"\s+(\w+) size of raw data"),
                FilePointerToRawData = Utils.ExtractValue(sectionText, @"\s+(\w+) file pointer to raw data"),
                FilePointerToRelocationTable = Utils.ExtractValue(sectionText, @"\s+(\w+) file pointer to relocation table"),
                FilePointerToLineNumbers = Utils.ExtractValue(sectionText, @"\s+(\w+) file pointer to line numbers")
            });
                
        }
        
        return sections;
    }
    
    public static Function GetFunctionBytes(string inputExe, Section section, Dictionary<string, PublicSymbol> publicSymbolsDict, PublicSymbol symbol)
    {
        var nextFunctionAddress = FindNextLowestAddress(publicSymbolsDict, symbol)!.Value.Address;
            
        // Load the exe into a byte array
        byte[] exeBytes = File.ReadAllBytes(inputExe);
            
        // Get the offset of the section
        int sectionOffset = int.Parse(section.FilePointerToRawData, System.Globalization.NumberStyles.HexNumber);
            
        // Get the offset of the public symbol in the exe
        int publicOffsetInExe = sectionOffset + symbol.Address;
            
        var functionSize = nextFunctionAddress - symbol.Address;
            
        // Read first 4 bytes of the public symbol
        byte[] publicFunctionBytes = new byte[functionSize];
        Array.Copy(exeBytes, publicOffsetInExe, publicFunctionBytes, 0, publicFunctionBytes.Length);
        
        return new Function()
        {
            Bytes = publicFunctionBytes,
            Address = (ulong) publicOffsetInExe
        };
    }
    
    public static void WriteASM(byte[] bytes, ulong offsetInExe)
    {
        const int exampleCodeBitness = 64;
        // You can also pass in a hex string, eg. "90 91 929394", or you can use your own CodeReader
        // reading data from a file or memory etc
        var codeBytes = bytes;
        var codeReader = new ByteArrayCodeReader(codeBytes);
        var decoder = Decoder.Create(exampleCodeBitness, codeReader);
        decoder.IP = offsetInExe;
        ulong endRip = decoder.IP + (uint)codeBytes.Length;

        var instructions = new List<Instruction>();
        while (decoder.IP < endRip)
            instructions.Add(decoder.Decode());
            
        // Formatters: Masm*, Nasm*, Gas* (AT&T) and Intel* (XED).
        // There's also `FastFormatter` which is ~2x faster. Use it if formatting speed is more
        // important than being able to re-assemble formatted instructions.
        var formatter = new NasmFormatter();
        formatter.Options.DigitSeparator = "";
        formatter.Options.FirstOperandCharIndex = 10;
        var output = new StringOutput();
        foreach (var instr in instructions) {
            // Don't use instr.ToString(), it allocates more, uses masm syntax and default options
            formatter.Format(instr, output);
            Console.Write(instr.IP.ToString("X16"));
            Console.Write(" ");
            int instrLen = instr.Length;
            int byteBaseIndex = (int)(instr.IP - offsetInExe);
            for (int i = 0; i < instrLen; i++)
                Console.Write(codeBytes[byteBaseIndex + i].ToString("X2"));
            int missingBytes = 10 - instrLen;
            for (int i = 0; i < missingBytes; i++)
                Console.Write("  ");
            Console.Write(" ");
            Console.WriteLine(output.ToStringAndReset());
        }
    }
    
    public static PublicSymbol? FindNextLowestAddress(Dictionary<string, PublicSymbol> set, PublicSymbol currentSymbol)
    {
        // Go through ALL the public symbols (Since its not sorted at all) and find the lowest address more than the current symbol's address

        PublicSymbol currentSmallest = new PublicSymbol()
        {
            Address = int.MaxValue
        };
            
        foreach (var symbol in set)
        {
            if (symbol.Value.Address > currentSymbol.Address)
            {
                if (symbol.Value.Address < currentSmallest.Address)
                    currentSmallest = symbol.Value;
            }
        }

        return currentSmallest;
    }
    public static string ExtractValue(string input, string pattern)
    {
        Match match = Regex.Match(input, pattern, RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }
}
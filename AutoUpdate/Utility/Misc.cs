using System.Diagnostics;
using AutoUpdate.Structs;

namespace AutoUpdate.Utility;

public class Misc
{
    
    public static void RunProcess(string? filename, string arguments)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException($"File not found: {filename}");
        }
        
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.Arguments = arguments;
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = false;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine($"\"{filename}\" {arguments}");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        
        cmd.WaitForExit();
    }

    public static string RunProcessAndGetOutput(string filename, string arguments)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException($"File not found: {filename}");
        }
        
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.Arguments = arguments;
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = false;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();
        
        cmd.StandardInput.WriteLine($"\"{filename}\" {arguments}");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        string output = cmd.StandardOutput.ReadToEnd();
        cmd.WaitForExit();
        return output;
    }
    
    public static Function GetFunctionBytes(Section section, PublicSymbol symbol)
    {
        var nextFunctionAddress = FindNextLowestAddress(symbol)!.Value.Address;
            
        // Load the exe into a byte array
        byte[] exeBytes = File.ReadAllBytes(Program.Config.ExeFile);
            
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
    
    
    
    public static PublicSymbol? FindNextLowestAddress(PublicSymbol currentSymbol)
    {
        // Go through ALL the public symbols (Since its not sorted at all) and find the lowest address more than the current symbol's address

        PublicSymbol currentSmallest = new PublicSymbol()
        {
            Address = int.MaxValue
        };
            
        foreach (var symbol in Program.PublicSymbolsDict)
        {
            if (symbol.Value.Address > currentSymbol.Address)
            {
                if (symbol.Value.Address < currentSmallest.Address)
                    currentSmallest = symbol.Value;
            }
        }

        return currentSmallest;
    }

    public static string GetVersion()
    {
        return "Unknown";
    }
}
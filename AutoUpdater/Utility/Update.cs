using AutoUpdater.Models;
using AutoUpdater.Resolvers;
using AutoUpdater.Resolvers.Impl;

namespace AutoUpdater.Utility;

public static class Update
{
    public static Dictionary<string, int> UpdateFromConfig(Configs.Config config)
    {
        Dictionary<string, int> offsets = new();
        
        foreach (var item in config.SDK.Items)
        {
            
            // try
            // {
            //     // ??_7Actor@@6B@ is the public symbol for the Actor class's destructor
            //     symbol = Program.PublicSymbolsDict[item.Function];
            // } catch (KeyNotFoundException)
            // {
            //     Console.WriteLine($"Function {item.Function} not found.");
            //     continue;
            // }

            if (!Program.PublicSymbolsDict.TryGetValue(item.Function, out var symbol))
            {
                Console.WriteLine($"Function {item.Function} not found.");
                continue;
            }
            
            // string section index = 0002:1908168 (the 0002 is the section index)
            
            var section = Program.Sections.Find(x => x.Index == symbol.SectionIndex);
            
            Console.WriteLine("==========================================");
            
            // Print out info about the public symbol
            Console.WriteLine($"Name: {LLVM.Demangle(symbol.Name)}");/*
            Console.WriteLine($"Address: {symbol.Address}");
            Console.WriteLine($"Section: {section.Name}");
            Console.WriteLine($"Section Address: {section.VirtualAddress}");
            Console.WriteLine($"Section Size: {section.VirtualSize}");
            Console.WriteLine($"Section Raw Data Size: {section.SizeOfRawData}");
            Console.WriteLine($"Section Raw Data Pointer: {section.FilePointerToRawData}");
            Console.WriteLine($"Section Relocation Table Pointer: {section.FilePointerToRelocationTable}");
            Console.WriteLine($"Section Line Numbers Pointer: {section.FilePointerToLineNumbers}");*/
            
            
            var function = Misc.GetFunctionBytes(section, symbol);
            
            // Print out the bytes
            //Console.WriteLine($"Bytes: {BitConverter.ToString(function.Bytes)}");
            //Iced.OutputAsm(function.Bytes, function.Address);

            var offset = item.Resolver.Resolve(function.Bytes);
            
            if (offset == -1)
            {
                Console.WriteLine("Resolver failed.");
                continue;
            }
            
            //Console.WriteLine($"Offset: {offset}");
            // Read 4 bytes at the offset
            var offsetBytes = new byte[4];
            Array.Copy(function.Bytes, offset, offsetBytes, 0, offsetBytes.Length);
            
            // Convert the bytes to an int
            var offsetInt = BitConverter.ToInt32(offsetBytes, 0);
            
            // Write as hex
            Console.WriteLine($"{item.Name}: {offsetInt:X}");
            
            offsets.Add(item.Name, offsetInt);
        }
        
        return offsets;
    }
}
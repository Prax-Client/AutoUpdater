﻿using AutoUpdate.Models;
using AutoUpdate.Resolvers;
using AutoUpdate.Resolvers.Impl;
using AutoUpdate.Structs;

namespace AutoUpdate.Utility;

public class Update
{
    public static void UpdateFromConfig(ConfigModel config)
    {
        foreach (var item in config.UpdateItems)
        {

            PublicSymbol symbol;
            
            try
            {
                
                symbol =
                    Program.PublicSymbolsDict[
                        item.Function
                    ]; // ??_7Actor@@6B@ is the public symbol for the Actor class's destructor

            } catch (KeyNotFoundException)
            {
                Console.WriteLine($"Function {item.Function} not found.");
                continue;
            }
            
            // string section index = 0002:1908168 (the 0002 is the section index)
            
            Section section = Program.Sections.Find(x => x.Index == symbol.SectionIndex);
            
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

            int offset = item.Resolver.Resolve(function.Bytes);
            
            
            //Console.WriteLine($"Offset: {offset}");
            // Read 4 bytes at the offset
            byte[] offsetBytes = new byte[4];
            Array.Copy(function.Bytes, offset, offsetBytes, 0, offsetBytes.Length);
            
            // Convert the bytes to an int
            int offsetInt = BitConverter.ToInt32(offsetBytes, 0);
            
            // Write as hex
            Console.WriteLine($"{item.Name}: {offsetInt:X}");
        }
    }
}
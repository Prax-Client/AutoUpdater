using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Iced.Intel;
using AutoUpdate.Structs;

namespace AutoUpdate 
{
    internal class Program
    {
        
        public static string inputFile = @"C:\Users\Flash\Downloads\bedrock-server-1.20.32.03\bedrock_server.pdb";
        public static string inputExe = @"C:\Users\Flash\Downloads\bedrock-server-1.20.32.03\bedrock_server.exe";
        public static string pdbutil = @"C:\Program Files\LLVM\bin\llvm-pdbutil.exe";
        public static string undname = @"C:\Program Files\LLVM\bin\llvm-undname.exe";
        
        static void Main(string[] args)
        {
            Console.Title = "Auto Updater";
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            
            var publicSymbolsDict = Utils.DumpPublics(pdbutil, inputFile);
            var sections = Utils.DumpSections(pdbutil, inputFile);
            
            Console.Write("Enter a public symbol name: ");
            
            PublicSymbol actor =
                publicSymbolsDict[
                    Console.ReadLine()];
            
            // string section index = 0002:1908168 (the 0002 is the section index)
            
            Section section = sections.Find(x => x.Index == actor.SectionIndex);
            
            // Print out info about the public symbol
            Console.WriteLine($"Name: {actor.Name}");
            Console.WriteLine($"Address: {actor.Address}");
            Console.WriteLine($"Section: {section.Name}");
            Console.WriteLine($"Section Address: {section.VirtualAddress}");
            Console.WriteLine($"Section Size: {section.VirtualSize}");
            Console.WriteLine($"Section Raw Data Size: {section.SizeOfRawData}");
            Console.WriteLine($"Section Raw Data Pointer: {section.FilePointerToRawData}");
            Console.WriteLine($"Section Relocation Table Pointer: {section.FilePointerToRelocationTable}");
            Console.WriteLine($"Section Line Numbers Pointer: {section.FilePointerToLineNumbers}");
            
            
            var function = Utils.GetFunctionBytes(inputExe, section, publicSymbolsDict, actor);
            
            // Print out the bytes
            Console.WriteLine($"Bytes: {BitConverter.ToString(function.Bytes)}");
            Utils.WriteASM(function.Bytes, function.Address);
            
            Console.ReadLine();
            
            Console.Clear();
            
            
            
            Main(args);
        }
    }
}
using Iced.Intel;

namespace AutoUpdater.Utility;

public sealed class SymbolResolver : ISymbolResolver {
    private readonly Dictionary<ulong, string> _symbolDict;

    public SymbolResolver(Dictionary<ulong, string> symbolDict) {
        _symbolDict = symbolDict;
    }

    public bool TryGetSymbol(in Instruction instruction, int operand, int instructionOperand,
        ulong address, int addressSize, out SymbolResult symbol) {
        if (_symbolDict.TryGetValue(address, out var symbolText)) {
            // The 'address' arg is the address of the symbol and doesn't have to be identical
            // to the 'address' arg passed to TryGetSymbol(). If it's different from the input
            // address, the formatter will add +N or -N, eg. '[rax+symbol+123]'
            symbol = new SymbolResult(address, symbolText);
            return true;
        }
        symbol = default;
        return false;
    }
}

public static class Iced
{
    public static void OutputAsm(byte[] bytes, ulong offsetInExe)
    {
        const int exampleCodeBitness = 64;
        // You can also pass in a hex string, eg. "90 91 929394", or you can use your own CodeReader
        // reading data from a file or memory etc
        var codeReader = new ByteArrayCodeReader(bytes);
        var decoder = Decoder.Create(exampleCodeBitness, codeReader);
        decoder.IP = offsetInExe;
        var endRip = decoder.IP + (uint)bytes.Length;

        var instructions = new List<Instruction>();
        while (decoder.IP < endRip)
            instructions.Add(decoder.Decode());
            
        // Formatters: Masm*, Nasm*, Gas* (AT&T) and Intel* (XED).
        // There's also `FastFormatter` which is ~2x faster. Use it if formatting speed is more
        // important than being able to re-assemble formatted instructions.
        
        //var symbolResolver = new SymbolResolver(LLVM.GetSymbolsForSymbolResolver());
        
        var formatter = new NasmFormatter(/*symbolResolver*/)
        {
            Options =
            {
                DigitSeparator = "",
                FirstOperandCharIndex = 10
            }
        };
        var output = new StringOutput();
        foreach (var instr in instructions) {
            // Don't use instr.ToString(), it allocates more, uses masm syntax and default options
            formatter.Format(instr, output);
            Console.Write($"{instr.IP:X16} ");
            
            var instrLen = instr.Length;
            var byteBaseIndex = (int)(instr.IP - offsetInExe);
            for (var i = 0; i < instrLen; i++)
                Console.Write($"{bytes[byteBaseIndex + i]:X2}");
            
            var missingBytes = 10 - instrLen;
            for (var i = 0; i < missingBytes; i++)
                Console.Write("  ");
            
            Console.Write(" ");
            Console.WriteLine(output.ToStringAndReset());
        }
    }
}
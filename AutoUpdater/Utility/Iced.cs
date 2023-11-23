using Iced.Intel;

namespace AutoUpdater.Utility;

sealed class SymbolResolver : ISymbolResolver {
    readonly Dictionary<ulong, string> symbolDict;

    public SymbolResolver(Dictionary<ulong, string> symbolDict) {
        this.symbolDict = symbolDict;
    }

    public bool TryGetSymbol(in Instruction instruction, int operand, int instructionOperand,
        ulong address, int addressSize, out SymbolResult symbol) {
        if (symbolDict.TryGetValue(address, out var symbolText)) {
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

public class Iced
{
    public static void OutputAsm(byte[] bytes, ulong offsetInExe)
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
        
        //var symbolResolver = new SymbolResolver(LLVM.GetSymbolsForSymbolResolver());
        
        var formatter = new NasmFormatter(/*symbolResolver*/);
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
}
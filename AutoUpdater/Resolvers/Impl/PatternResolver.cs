namespace AutoUpdater.Resolvers.Impl;

public class PatternResolver : Resolver
{
    public static byte[] StringToPattern(string input)
    {
        // Split the input into an array of strings
        var inputSplit = input.Split(" ");
        // If the input is a ? convert to 0xFE (wildcard)
        for (var i = 0; i < inputSplit.Length; i++)
        {
            if (inputSplit[i] == "?")
            {
                inputSplit[i] = "FE";
            }
        }
        
        // Convert the string array to a byte array
        var pattern = new byte[inputSplit.Length];
        for (var i = 0; i < inputSplit.Length; i++)
        {
            pattern[i] = Convert.ToByte(inputSplit[i], 16);
        }
        
        return pattern;
    }
    
    public override int Resolve(byte[] haystack)
    {
        var needle = StringToPattern(Value);

        var needleLength = needle.Length;
        var haystackLength = haystack.Length;
        var maxIndex = haystackLength - needleLength;
        for (var i = 0; i <= maxIndex; i++)
        {
            var found = true;
            for (var j = 0; j < needleLength; j++)
            {
                if (haystack[i + j] != needle[j] && needle[j] != 0xFE)
                {
                    found = false;
                    break;
                }
            }
            if (found)
            {
                return i;
            }
        }
        return -1;
    }
    
    public PatternResolver()
    {
        Type = ResolverType.Pattern;
    }
    
    public PatternResolver(string value)
    {
        Type = ResolverType.Pattern;
        Value = value;
    }
}
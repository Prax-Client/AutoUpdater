namespace AutoUpdate.Resolvers.Impl;

public class PatternResolver : Resolver
{
    public static byte[] StringToPattern(string input)
    {
        // Split the input into an array of strings
        string[] inputSplit = input.Split(" ");
        // If the input is a ? convert to 0xFE (wildcard)
        for (int i = 0; i < inputSplit.Length; i++)
        {
            if (inputSplit[i] == "?")
            {
                inputSplit[i] = "FE";
            }
        }
        
        // Convert the string array to a byte array
        byte[] pattern = new byte[inputSplit.Length];
        for (int i = 0; i < inputSplit.Length; i++)
        {
            pattern[i] = Convert.ToByte(inputSplit[i], 16);
        }
        
        return pattern;
    }
    
    public override int Resolve(byte[] bytes)
    {
        byte[] needle = StringToPattern(Value);
        byte[] haystack = bytes;
        
        int needleLength = needle.Length;
        int haystackLength = haystack.Length;
        int maxIndex = haystackLength - needleLength;
        for (int i = 0; i <= maxIndex; i++)
        {
            bool found = true;
            for (int j = 0; j < needleLength; j++)
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
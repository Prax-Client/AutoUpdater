using AutoUpdater.Utility;

namespace AutoUpdater.Resolvers;

public enum ResolverType
{
    Pattern,
    Offset
}

public class Resolver 
{
    public ResolverType Type = ResolverType.Pattern;
    public string Value = ""; // This will either be a pattern or an offset e.g ? ? ? ? C3 or 3 for 3 byte offset from the start of the function
    
    public Resolver()
    {
        
    }
    
    public Resolver(ResolverType type, string value)
    {
        Type = type;
        Value = value;
    }
    
    public virtual int Resolve(byte[] haystack)
    {
        throw new NotImplementedException();
    }
}
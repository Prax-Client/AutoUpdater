namespace AutoUpdater.Resolvers.Impl;

public class OffsetResolver : Resolver
{
    public override int Resolve(byte[] haystack)
    {
        return int.Parse(Value);
    }
    
    public OffsetResolver()
    {
        Type = ResolverType.Offset;
    }
    
    public OffsetResolver(string value)
    {
        Type = ResolverType.Offset;
        Value = value;
    }
}
namespace AttributeNetworkWrapper.Core;

public static class Extensions
{
    //fnv1a hashing folded to 16bits
    public static ushort GetStableHashCode(this string text)
    {
        unchecked
        {
            uint hash = 0x811c9dc5;
            uint prime = 0x1000193;

            foreach (var t in text)
            {
                byte value = (byte)t;
                hash ^= value;
                hash *= prime;
            }
                
            return (ushort)((hash >> 16) ^ hash);
        }
    }
}
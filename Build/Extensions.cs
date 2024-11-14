using Mono.Cecil;

namespace Build;

public static class Extensions
{
    public static bool IsType<T>(this TypeReference typeRef)
    {
        return typeRef.FullName == typeof(T).FullName;
    }
}
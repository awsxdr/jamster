namespace amethyst.Extensions;

internal static class TypeExtensions
{
    public static IEnumerable<Type> BaseTypes(this Type type)
    {
        var current = type;

        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

    public static bool IsDerivedFrom(this Type type, Type testType) =>
        type.BaseTypes().Any(
            testType.IsGenericTypeDefinition
                ? (t => t.IsGenericType && t.GetGenericTypeDefinition() == testType)
                : (t => t == testType));

    public static bool IsDerivedFrom(this Type type, Type testType, out Type? derivedType)
    {
        derivedType = type.BaseTypes().FirstOrDefault(
            testType.IsGenericTypeDefinition
                ? (t => t.IsGenericType && t.GetGenericTypeDefinition() == testType)
                : (t => t == testType));

        return derivedType is not null;
    }
}
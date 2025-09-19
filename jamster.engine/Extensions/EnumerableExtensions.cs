namespace jamster.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TItem> Pad<TItem>(this IEnumerable<TItem> @this, int size, TItem padWith)
    {
        var items = @this.ToArray();

        return items.Concat(Enumerable.Repeat(padWith, Math.Max(0, size - items.Length)));
    }
}
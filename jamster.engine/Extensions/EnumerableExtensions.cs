namespace jamster.engine.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TItem> Pad<TItem>(this IEnumerable<TItem> @this, int size, TItem padWith)
    {
        var items = @this.ToArray();

        return items.Concat(Enumerable.Repeat(padWith, Math.Max(0, size - items.Length)));
    }

    public static int IndexOf<TItem>(this IEnumerable<TItem> @this, Func<TItem, bool> predicate) =>
        @this.Select((item, index) => (item, index))
            .FirstOrDefault(x => predicate(x.item), (default, -1)!)
            .index;

    public static int IndexOfMax<TItem>(this IEnumerable<TItem> @this) where TItem : IComparable<TItem> =>
        @this.Select((item, index) => (item, index))
            .OrderBy(i => i.item)
            .Select(i => i.index)
            .Prepend(-1)
            .Last();

    public static int IndexOfMax<TItem, TKey>(this IEnumerable<TItem> @this, Func<TItem, TKey> keySelector) where TKey : IComparable<TKey> =>
        @this.Select(keySelector).IndexOfMax();

    public static IEnumerable<TItem> Difference<TItem>(this IEnumerable<TItem> @this, IEnumerable<TItem> other)
        where TItem : IEquatable<TItem>
    {
        @this = @this.ToArray();
        other = other.ToArray();

        return @this.Union(other).Except(@this.Intersect(other));
    }
}
namespace amethyst.Extensions;

public static class CollectionExtensions
{
    public static T? Random<T>(this ICollection<T> collection) =>
        collection.Any()
            ? collection.ElementAt(System.Random.Shared.Next(collection.Count))
            : default;

    public static IEnumerable<T> Shuffle<T>(this ICollection<T> collection)
    {
        var items = collection.ToList();
        var result = new List<T>();

        while (items.Any())
        {
            var index = System.Random.Shared.Next(0, items.Count);
            result.Add(items[index]);
            items.RemoveAt(index);
        }

        return result;
    }
}
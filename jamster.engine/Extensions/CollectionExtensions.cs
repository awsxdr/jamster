namespace jamster.engine.Extensions;

public static class CollectionExtensions
{
    public static T? Random<T>(this ICollection<T> collection) =>
        collection.Any()
            ? collection.ElementAt(System.Random.Shared.Next(collection.Count))
            : default;

    public static T? RandomFavorStart<T>(this ICollection<T> collection) =>
        Enumerable.Range(1, collection.Count)
            .Reverse()
            .Zip(collection)
            .SelectMany(x => Enumerable.Repeat(x.Second, x.First))
            .ToArray()
            .Random();

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
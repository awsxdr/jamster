using jamster.engine.Domain;

namespace jamster.engine.tests;

public static class DataGenerator
{
    public static Tick GetRandomTick() => Random.Shared.NextInt64(Tick.MaxValue / 2);
    public static Tick GetRandomTickFollowing(long previousTick) => Random.Shared.NextInt64(previousTick + 1, previousTick + (Tick.MaxValue - previousTick) / 2);

    public static int[] GetRandomIntArray(int count, int min = int.MinValue, int max = int.MaxValue) =>
        Enumerable.Range(0, count).Select(_ => Random.Shared.Next(min, max)).ToArray();
}
using amethyst.Domain;

namespace amethyst.tests;

public static class DataGenerator
{
    public static Tick GetRandomTick() => Random.Shared.NextInt64(Tick.MaxValue / 2);
    public static Tick GetRandomTickFollowing(long previousTick) => Random.Shared.NextInt64(previousTick + 1, previousTick + (Tick.MaxValue - previousTick) / 2);
}
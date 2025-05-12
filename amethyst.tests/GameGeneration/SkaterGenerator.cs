using amethyst.Domain;
using amethyst.Extensions;
using Func;

namespace amethyst.tests.GameGeneration;

public static class SkaterGenerator
{
    private const float PenaltyChance = 1.0f / 1200.0f;

    public static SimulatorSkater GenerateRandom() => new(
        new(GetRandomNumber(), NameGenerator.GetRandomName()),
        GetRandomPosition(),
        GetRandomSpeed(),
        GetRandomPenaltyChance()
    );

    private static string GetRandomNumber() =>
        Enumerable.Range(0, Enumerable.Range(0, 4).ToArray().RandomFavorStart() + 1)
            .Select(_ => Random.Shared.Next(10).ToString())
            .Map(string.Concat);

    private static SkaterPosition GetRandomPosition() =>
        Random.Shared.Next(3) switch
        {
            0 => SkaterPosition.Jammer,
            1 => SkaterPosition.Pivot,
            _ => SkaterPosition.Blocker,
        };

    private static float GetRandomSpeed() =>
        Random.Shared.NextSingle() * 2.0f + 3.0f;

    private static float GetRandomPenaltyChance() =>
        (Random.Shared.NextSingle() + 1.0f) * PenaltyChance;

}

public record SimulatorSkater(Skater DomainSkater, SkaterPosition FavoredPosition, float BaseSpeed, float PenaltyChance)
{
    public string Name => DomainSkater.Name;
    public string Number => DomainSkater.Number;
}
namespace amethyst.tests.GameGeneration;

public record GeneratorValues(float PenaltyChance)
{
    public static GeneratorValues Default => new(1f / 1200f);
}
namespace amethyst.tests.GameGeneration;

public static class GameGenerator
{
    public static SimulatorGame GenerateRandom()
    {
        var homeTeam = TeamGenerator.GenerateRandom();
        var awayTeam = TeamGenerator.GenerateRandom();

        return new(
            homeTeam,
            awayTeam
        );
    }
}

public record SimulatorGame(SimulatorTeam HomeTeam, SimulatorTeam AwayTeam);
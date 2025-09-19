using jamster.Domain;

namespace jamster.engine.tests.GameGeneration;

public static class TeamGenerator
{
    public static SimulatorTeam GenerateRandom()
    {
        var roster = GetRandomRoster();

        var color = NameGenerator.GetRandomColor();

        var domainTeam = new Team(
            Guid.NewGuid(),
            new()
            {
                ["league"] = $"{NameGenerator.GetRandomPlace()} Roller Derby",
                ["color"] = color.Key,
            },
            new[] { color }.ToDictionary(),
            roster.Select(s => s.DomainSkater).ToArray(),
            DateTimeOffset.UtcNow);

        return new(domainTeam, roster);
    }

    private static SimulatorSkater[] GetRandomRoster()
    {
        var roster = new List<SimulatorSkater>();
        var targetCount = 8 + Random.Shared.Next(8);

        for (var i = 0; i < targetCount; ++i)
        {
            SimulatorSkater skater;

            do
            {
                skater = SkaterGenerator.GenerateRandom();
            } while (roster.Any(s => s.DomainSkater.Number == skater.DomainSkater.Number));

            roster.Add(skater);
        }

        return roster.OrderBy(s => s.DomainSkater.Number).ToArray();
    }
}

public record SimulatorTeam(Team DomainTeam, SimulatorSkater[] Roster);
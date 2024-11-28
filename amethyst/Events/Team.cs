using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class TeamSet(Guid7 id, TeamSetBody body) : Event<TeamSetBody>(id, body);
public sealed record TeamSetBody(TeamSide TeamSide, GameTeam Team) : TeamEventBody(TeamSide);

public sealed record GameTeam(Dictionary<string, string> Names, TeamColor Color, List<Skater> Roster)
{
    public bool Equals(GameTeam? other) =>
        other is not null
        && other.Names.SequenceEqual(Names)
        && other.Color.Equals(Color)
        && other.Roster.SequenceEqual(Roster);

    public override int GetHashCode() => 
        HashCode.Combine(Names, Color, Roster);
}
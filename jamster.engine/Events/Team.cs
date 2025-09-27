﻿using jamster.engine.Domain;
using jamster.engine.Services;

namespace jamster.engine.Events;

public sealed class TeamSet(Guid7 id, TeamSetBody body) : Event<TeamSetBody>(id, body);
public sealed record TeamSetBody(TeamSide TeamSide, GameTeam Team) : TeamEventBody(TeamSide);

public sealed record GameTeam(Dictionary<string, string> Names, TeamColor Color, List<GameSkater> Roster)
{
    public bool Equals(GameTeam? other) =>
        other is not null
        && other.Names.SequenceEqual(Names)
        && other.Color.Equals(Color)
        && other.Roster.SequenceEqual(Roster);

    public override int GetHashCode() => 
        HashCode.Combine(Names, Color, Roster);
}

public sealed record GameSkater(string Number, string Name, bool IsSkating);
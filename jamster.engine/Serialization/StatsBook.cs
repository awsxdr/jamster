namespace jamster.engine.Serialization;

public sealed record StatsBook(Igrf Igrf, ScoreSheetCollection ScoreSheets, PenaltySheetCollection PenaltySheets, LineupSheetCollection LineupSheets);


public sealed record Igrf(GameLocation Location, GameDetails GameDetails, GameTeams Teams, GameSummary GameSummary);
public sealed record GameLocation(string Venue, string City, string Province);
public sealed record GameDetails(string EventName, string GameNumber, string HostLeagueName, DateTime GameStart);
public sealed record GameTeams(StatsBookTeam HomeTeam, StatsBookTeam AwayTeam);

public sealed record StatsBookTeam(string LeagueName, string TeamName, string ColorName, StatsBookSkater[] Skaters)
{
    public bool Equals(StatsBookTeam? other) =>
        other != null
        && other.LeagueName.Equals(LeagueName)
        && other.TeamName.Equals(TeamName)
        && other.ColorName.Equals(ColorName)
        && other.Skaters.SequenceEqual(Skaters);

    public override int GetHashCode() => HashCode.Combine(LeagueName, TeamName, ColorName, Skaters);
}
public sealed record StatsBookSkater(string Number, string Name, bool IsSkating);

public sealed record GameSummary(PeriodSummary Period1Summary, PeriodSummary Period2Summary);
public sealed record PeriodSummary(int HomeTeamPenalties, int HomeTeamScore, int AwayTeamPenalties, int AwayTeamScore);

public sealed record Signatories(Signatory HomeTeamCaptain, Signatory AwayTeamCaptain, Signatory HeadReferee, Signatory HeadNso);
public sealed record Signatory(string SkateName, string LegalName);


public sealed record ScoreSheetCollection(
    ScoreSheet HomePeriod1,
    ScoreSheet AwayPeriod1,
    ScoreSheet HomePeriod2,
    ScoreSheet AwayPeriod2);
public sealed record ScoreSheet(string ScoreKeeper, string JammerRef, ScoreSheetLine[] Lines);

public sealed record ScoreSheetLine(
    Union<int, string> Jam,
    string JammerNumber,
    bool Lost,
    bool Lead,
    bool Call,
    bool Injury,
    bool NoInitial,
    ScoreSheetTrip[] Trips)
{
    public bool Equals(ScoreSheetLine? other) =>
        other is not null
        && other.Jam.Value.Equals(Jam.Value)
        && other.JammerNumber.Equals(JammerNumber)
        && other.Lost.Equals(Lost)
        && other.Lead.Equals(Lead)
        && other.Call.Equals(Call)
        && other.Injury.Equals(Injury)
        && other.NoInitial.Equals(NoInitial)
        && other.Trips.SequenceEqual(Trips);

    public override int GetHashCode() => HashCode.Combine(Jam, JammerNumber, Lost, Lead, Call, Injury, NoInitial, Trips);
}
public sealed record ScoreSheetTrip(int? Score);

public sealed record PenaltySheetCollection(
    PenaltySheet Period1,
    PenaltySheet Period2);
public sealed record PenaltySheet(string PenaltyTracker, PenaltySheetLine[] HomePenalties, PenaltySheetLine[] AwayPenalties);
public sealed record PenaltySheetLine(int Offset, string SkaterNumber, Penalty[] Penalties, Penalty? Expulsion);
public sealed record Penalty(int JamNumber, string Code);

public sealed record LineupSheetCollection(
    LineupSheet HomePeriod1,
    LineupSheet AwayPeriod1,
    LineupSheet HomePeriod2,
    LineupSheet AwayPeriod2);

public sealed record LineupSheet(string LineupTracker, LineupSheetLine[] Lines)
{
    public bool Equals(LineupSheet? other) =>
        other is not null
        && other.LineupTracker.Equals(LineupTracker)
        && other.Lines.SequenceEqual(Lines);

    public override int GetHashCode() => HashCode.Combine(LineupTracker, Lines);
}

public sealed record LineupSheetLine(Union<int, string> Jam, bool NoPivot, LineupSkater[] Skaters)
{
    public bool Equals(LineupSheetLine? other) =>
        other is not null
        && other.Jam.ToString().Equals(Jam.ToString())
        && other.NoPivot.Equals(NoPivot)
        && other.Skaters.SequenceEqual(Skaters);

    public override int GetHashCode() => HashCode.Combine(Jam, NoPivot, Skaters);
       
}

public sealed record LineupSkater(string? Number, string[] BoxSymbols)
{
    public bool Equals(LineupSkater? other) =>
        other is not null
        && (other.Number?.Equals(Number) ?? Number is null)
        && other.BoxSymbols.SequenceEqual(BoxSymbols);

    public override int GetHashCode() => HashCode.Combine(Number, BoxSymbols);
}
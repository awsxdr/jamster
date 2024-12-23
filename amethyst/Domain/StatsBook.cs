namespace amethyst.Domain;

public sealed record StatsBook(Igrf Igrf);

public sealed record Igrf(GameLocation Location, GameDetails GameDetails, GameTeams Teams, GameSummary GameSummary);

public sealed record GameLocation(string Venue, string City, string Province);

public sealed record GameDetails(string EventName, string GameNumber, string HostLeagueName, DateTime GameStart);

public sealed record GameTeams(StatsBookTeam HomeTeam, StatsBookTeam AwayTeam);
public sealed record StatsBookTeam(string LeagueName, string TeamName, string ColorName, StatsBookSkater[] Skaters);
public sealed record StatsBookSkater(string Number, string Name, bool IsSkating);

public sealed record GameSummary(PeriodSummary Period1Summary, PeriodSummary Period2Summary); 
public sealed record PeriodSummary(int HomeTeamPenalties, int HomeTeamScore, int AwayTeamPenalties, int AwayTeamScore);

public sealed record Signatories(Signatory HomeTeamCaptain, Signatory AwayTeamCaptain, Signatory HeadReferee, Signatory HeadNso);
public sealed record Signatory(string SkateName, string LegalName);

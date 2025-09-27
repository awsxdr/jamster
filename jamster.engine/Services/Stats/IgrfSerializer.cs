using System.IO.Compression;

using jamster.engine.Serialization;

namespace jamster.engine.Services.Stats;

public interface IIgrfSerializer
{
    Task<Result<ZipArchive>> SerializeIgrf(Igrf igrf, ZipArchive archive);
    Task<Result<Igrf>> DeserializeIgrf(ZipArchive archive);
}

[Singleton]
public class IgrfSerializer(ILogger<IgrfSerializer> logger) : StatsSheetSerializerBase(logger), IIgrfSerializer
{
    private const string IgrfSheetPath = "xl/worksheets/sheet2.xml";

    public Task<Result<ZipArchive>> SerializeIgrf(Igrf igrf, ZipArchive archive) =>
        GetWorksheetByEntryName(IgrfSheetPath, archive)
            .Then(WriteIgrf, igrf)
            .Then(UpdateSharedStrings, archive)
            .Then(WriteWorksheet)
            .Then(() => Result.Succeed(archive));

    public Task<Result<Igrf>> DeserializeIgrf(ZipArchive archive) =>
        GetWorksheetByEntryName(IgrfSheetPath, archive)
            .ThenMap(ReadIgrf);

    private static Igrf ReadIgrf(Worksheet igrfSheet) =>
        new(
            ReadGameLocation(igrfSheet),
            ReadGameDetails(igrfSheet),
            ReadGameTeams(igrfSheet),
            ReadGameSummary(igrfSheet)
        );

    private static Result<Worksheet> WriteIgrf(Igrf igrf, Worksheet igrfSheet) =>
        WriteGameLocation(igrf.Location, igrfSheet)
            .Then(WriteGameDetails, igrf.GameDetails)
            .Then(WriteGameTeams, igrf.Teams);

    private static GameLocation ReadGameLocation(Worksheet igrfSheet) =>
        new(
            GetCellValue(igrfSheet, 1, 3),
            GetCellValue(igrfSheet, 8, 3),
            GetCellValue(igrfSheet, 10, 3)
        );

    private static Result<Worksheet> WriteGameLocation(GameLocation gameLocation, Worksheet igrfSheet) =>
        SetCellValue(1, 3, gameLocation.Venue, igrfSheet)
            .Then(SetCellValue, 8, 3, gameLocation.City)
            .Then(SetCellValue, 10, 3, gameLocation.Province);

    private static GameDetails ReadGameDetails(Worksheet igrfSheet) =>
        new(
            GetCellValue(igrfSheet, 1, 5),
            GetCellValue(igrfSheet, 11, 3),
            GetCellValue(igrfSheet, 8, 5),
            ReadGameStart(igrfSheet)
        );

    private static Result<Worksheet> WriteGameDetails(GameDetails gameDetails, Worksheet igrfSheet) =>
        SetCellValue(1, 5, gameDetails.EventName, igrfSheet)
            .Then(SetCellValue, 11, 3, gameDetails.GameNumber)
            .Then(SetCellValue, 8, 5, gameDetails.HostLeagueName)
            .Then(WriteGameStart, gameDetails.GameStart);

    private static DateTime ReadGameStart(Worksheet igrfSheet)
    {
        // Dates and times are stored in XLSX as real numbers. The integer part is the number of days since 1900-01-01
        // and the fractional part is the time represented as a fraction of a day.
        var date = GetCellValue(igrfSheet, 1, 7);
        var time = GetCellValue(igrfSheet, 8, 7);

        var parsedValue = double.TryParse(date, out var d) && double.TryParse(time, out var t) ? d + t : 0.0;

        return DateTime.FromOADate(parsedValue);
    }

    private static Result<Worksheet> WriteGameStart(DateTime gameStart, Worksheet igrfSheet)
    {
        var oaDate = gameStart.ToOADate();

        return SetCellValue(1, 7, Math.Truncate(oaDate), igrfSheet)
            .Then(SetCellValue, 8, 7, oaDate - Math.Truncate(oaDate));
    }

    private static GameTeams ReadGameTeams(Worksheet igrfSheet) =>
        new(
            ReadTeam(igrfSheet, 1),
            ReadTeam(igrfSheet, 8)
        );

    private static Result<Worksheet> WriteGameTeams(GameTeams teams, Worksheet igrfSheet) =>
        WriteTeam(teams.HomeTeam, 1, igrfSheet)
            .Then(WriteTeam, teams.AwayTeam, 8);

    private static StatsBookTeam ReadTeam(Worksheet igrfSheet, int column) =>
        new(
            GetCellValue(igrfSheet, column, 10),
            GetCellValue(igrfSheet, column, 11),
            GetCellValue(igrfSheet, column, 12),
            ReadRoster(igrfSheet, column)
        );

    private static Result<Worksheet> WriteTeam(StatsBookTeam team, int column, Worksheet igrfSheet) =>
        SetCellValue(column, 10, team.LeagueName, igrfSheet)
            .Then(SetCellValue, column, 11, team.TeamName)
            .Then(SetCellValue, column, 12, team.ColorName)
            .Then(WriteRoster, team.Skaters, column);

    private static StatsBookSkater[] ReadRoster(Worksheet igrfDocument, int column) =>
        Enumerable.Range(0, 20)
            .Select(i =>
            {
                var number = GetCellValue(igrfDocument, column, 14 + i);

                return new StatsBookSkater(
                    number.TrimEnd('*'),
                    GetCellValue(igrfDocument, column + 1, 14 + i),
                    !number.Contains('*')
                );
            })
            .Where(skater => !string.IsNullOrWhiteSpace(skater.Number))
            .ToArray();

    private static Result<Worksheet> WriteRoster(StatsBookSkater[] roster, int column, Worksheet igrfDocument) =>
        roster.Aggregate(
            (Result: Result.Succeed(igrfDocument), Index: 0),
            (x, skater) => x.Result
                .Then(SetCellValue, column, 14 + x.Index, skater.Number + (skater.IsSkating ? "" : "*"))
                .Then(SetCellValue, column + 1, 14 + x.Index, skater.Name)
                .Map(r => (r, x.Index + 1)))
            .Result;

    private static GameSummary ReadGameSummary(Worksheet igrfDocument) =>
        new(
            ReadPeriodSummary(igrfDocument, 36),
            ReadPeriodSummary(igrfDocument, 37)
        );

    private static PeriodSummary ReadPeriodSummary(Worksheet igrfDocument, int row)
    {
        return new(
            ParseOrDefault(GetCellValue(igrfDocument, 5, row)),
            ParseOrDefault(GetCellValue(igrfDocument, 2, row)),
            ParseOrDefault(GetCellValue(igrfDocument, 11, row)),
            ParseOrDefault(GetCellValue(igrfDocument, 9, row))
        );

        int ParseOrDefault(string value) => int.TryParse(value, out var v) ? v : default;
    }
}
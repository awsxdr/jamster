using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using amethyst.Domain;
using Func;

namespace amethyst.Services;

public interface IStatsBookSerializer
{
    Task<Result<StatsBook>> DeserializeStream(Stream stream);
}

public class StatsBookSerializer(ILogger<StatsBookSerializer> logger) : IStatsBookSerializer
{
    private const string IgrfSheetPath = "xl/worksheets/sheet2.xml";

    public Task<Result<StatsBook>> DeserializeStream(Stream stream) =>
        ReadArchiveStream(stream)
            .Then(Deserialize);

    private Result<ZipArchive> ReadArchiveStream(Stream stream)
    {
        try
        {
            return Result.Succeed(new ZipArchive(stream, ZipArchiveMode.Read));
        }
        catch (InvalidDataException)
        {
            logger.LogWarning("File uploaded was invalid zip file");
            return Result<ZipArchive>.Fail<InvalidStatsBookFileFormatError>();
        }
    }

    private Task<Result<StatsBook>> Deserialize(ZipArchive archive) =>
        DeserializeIgrf(archive)
            .ThenMap(igrf => new StatsBook(igrf));

    private Task<Result<Igrf>> DeserializeIgrf(ZipArchive archive) =>
        GetEntry(archive, IgrfSheetPath)
            .Then(LoadDocumentFromEntry)
            .Then(document => 
                GetSharedStrings(archive).ThenMap(sharedStrings => new ReadableSheet(document, sharedStrings)))
            .ThenMap(ReadIgrf);

    private Igrf ReadIgrf(ReadableSheet igrfSheet) =>
        new(
            ReadGameLocation(igrfSheet),
            ReadGameDetails(igrfSheet),
            ReadGameTeams(igrfSheet),
            ReadGameSummary(igrfSheet)
        );

    private static GameLocation ReadGameLocation(ReadableSheet igrfSheet) =>
        new(
            GetCellValue(igrfSheet, 1, 3),
            GetCellValue(igrfSheet, 8, 3),
            GetCellValue(igrfSheet, 10, 3)
        );

    private static GameDetails ReadGameDetails(ReadableSheet igrfSheet) =>
        new(
            GetCellValue(igrfSheet, 1, 5),
            GetCellValue(igrfSheet, 11, 3),
            GetCellValue(igrfSheet, 8, 5),
            ReadGameStart(igrfSheet)
        );

    private static DateTime ReadGameStart(ReadableSheet igrfSheet)
    {
        // Dates and times are stored in XLSX as real numbers. The integer part is the number of days since 1900-01-01 and the fractional part is the time represented as a fraction of a day.
        var date = GetCellValue(igrfSheet, 1, 7);
        var time = GetCellValue(igrfSheet, 8, 7);

        var parsedValue = double.TryParse(date, out var d) && double.TryParse(time, out var t) ? d + t : 0.0;

        return DateTime.FromOADate(parsedValue);
    }

    private static GameTeams ReadGameTeams(ReadableSheet igrfSheet) =>
        new(
            ReadTeam(igrfSheet, 1),
            ReadTeam(igrfSheet, 8)
        );

    private static StatsBookTeam ReadTeam(ReadableSheet igrfSheet, int column) =>
        new(
            GetCellValue(igrfSheet, column, 10),
            GetCellValue(igrfSheet, column, 11),
            GetCellValue(igrfSheet, column, 12),
            ReadRoster(igrfSheet, column)
        );

    private static StatsBookSkater[] ReadRoster(ReadableSheet igrfDocument, int column) =>
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

    private GameSummary ReadGameSummary(ReadableSheet igrfDocument) =>
        new(
            ReadPeriodSummary(igrfDocument, 36),
            ReadPeriodSummary(igrfDocument, 37)
        );

    private PeriodSummary ReadPeriodSummary(ReadableSheet igrfDocument, int row)
    {
        return new(
            ParseOrDefault(GetCellValue(igrfDocument, 5, row)),
            ParseOrDefault(GetCellValue(igrfDocument, 2, row)),
            ParseOrDefault(GetCellValue(igrfDocument, 11, row)),
            ParseOrDefault(GetCellValue(igrfDocument, 9, row))
        );

        int ParseOrDefault(string value) => int.TryParse(value, out var v) ? v : default;
    }

    private static string GetCellValue(ReadableSheet document, int column, int row)
    {
        if (document.Document.Root is null) return string.Empty;

        var root = document.Document.Root;
        var @namespace = root.Name.Namespace;

        var rowString = row.ToString();
        var columnString = GetColumnString(column);

        var cell = root
            .Element(@namespace + "sheetData")
            ?.Elements(@namespace + "row")
            .SingleOrDefault(e => e.Attribute("r")!.Value == rowString)
            ?.Elements(@namespace + "c")
            .SingleOrDefault(e => e.Attribute("r")!.Value == $"{columnString}{rowString}");

        if (cell is null) return string.Empty;

        var cellType = cell.Attribute("t");

        return cellType?.Value switch
        {
            "s" => cell.Element(@namespace + "v")?.Value.Map(ReadSharedString),
            "inlineStr" => cell.Element(@namespace + "is")?.Element(@namespace + "t")?.Value,
            _ => cell.Element(@namespace + "v")?.Value
        } ?? string.Empty;

        string ReadSharedString(string reference) =>
            int.TryParse(reference, out var index) && index < document.SharedStrings.Length
                ? document.SharedStrings[index]
                : string.Empty;
    }

    private static string GetColumnString(int column) =>
        column >= 26
        ? $"{(char)('A' + column / 26 - 1)}{(char)('A' + column % 26)}"
        : ((char)('A' + column)).ToString();

    private Result<ZipArchiveEntry> GetEntry(ZipArchive archive, string path)
    {
        try
        {
            var entry = archive.GetEntry(path);

            if (entry == null)
                return Result<ZipArchiveEntry>.Fail<InvalidStatsBookFileFormatError>();

            return Result.Succeed(entry);
        }
        catch (InvalidDataException)
        {
            logger.LogWarning("File uploaded was invalid zip file");
            return Result<ZipArchiveEntry>.Fail<InvalidStatsBookFileFormatError>();
        }
    }

    private async Task<Result<XDocument>> LoadDocumentFromEntry(ZipArchiveEntry entry)
    {
        await using var entryStream = entry.Open();

        try
        {
            var document = await XDocument.LoadAsync(entryStream, LoadOptions.None, default);

            return Result.Succeed(document);
        }
        catch (XmlException)
        {
            logger.LogWarning("File uploaded contained invalid XML");
            return Result<XDocument>.Fail<InvalidStatsBookFileFormatError>();
        }
    }

    private Task<Result<string[]>> GetSharedStrings(ZipArchive archive) =>
        GetEntry(archive, "xl/sharedStrings.xml")
            .Then(LoadDocumentFromEntry)
            .Then(document =>
            {
                if (document.Root is null)
                {
                    logger.LogWarning("Document uploaded did non contain a valid sharedStrings.xml");
                    return Result<string[]>.Fail<InvalidStatsBookFileFormatError>();
                }

                var @namespace = document.Root.GetDefaultNamespace();

                var sharedStrings = document.Root
                    .Elements(@namespace + "si")
                    .Select(element => element.Element(@namespace + "t")?.Value ?? string.Empty)
                    .ToArray();

                return Result.Succeed(sharedStrings);
            });

    private sealed record ReadableSheet(XDocument Document, string[] SharedStrings);

    public sealed class InvalidStatsBookFileFormatError : ResultError;
}
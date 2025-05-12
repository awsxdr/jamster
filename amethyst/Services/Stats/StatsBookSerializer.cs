using System.IO.Compression;
using amethyst.Serialization;

namespace amethyst.Services.Stats;

public interface IStatsBookSerializer
{
    Task<Result<StatsBook>> DeserializeStream(Stream stream);
    Task<Result<StatsBookInfo>> ValidateStream(Stream stream);
    Task<Result<byte[]>> Serialize(StatsBook statsBook);
}

[Singleton]
public class StatsBookSerializer(
    IStatsBookValidator validator,
    IIgrfSerializer igrfSerializer,
    IBlankStatsBookStore blankStatsBookStore,
    IScoreSheetSerializer scoreSheetSerializer,
    IPenaltySheetSerializer penaltySheetSerializer,
    ILineupSheetSerializer lineupSheetSerializer,
    ILogger<StatsBookSerializer> logger
) : IStatsBookSerializer
{

    public Task<Result<StatsBook>> DeserializeStream(Stream stream) =>
        ReadArchiveStream(stream)
            .Then(Deserialize);

    public Task<Result<StatsBookInfo>> ValidateStream(Stream stream) =>
        ReadArchiveStream(stream)
            .Then(validator.ValidateStatsBook);

    public async Task<Result<byte[]>> Serialize(StatsBook statsBook)
    {
        if (!blankStatsBookStore.BlankStatsBookPresent)
            return Result<byte[]>.Fail<BlankStatsBookNotConfiguredError>();

        var test = System.Text.Json.JsonSerializer.Serialize(statsBook);

        using var stream = new MemoryStream();
        await stream.WriteAsync(await File.ReadAllBytesAsync(BlankStatsBookStore.BlankStatsBookPath));
        stream.Position = 0;

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Update))
        {
            await Serialize(statsBook, archive);
        }

        await stream.FlushAsync();
        return Result.Succeed(stream.ToArray());
    }


    private Task<Result<ZipArchive>> Serialize(StatsBook statsBook, ZipArchive archive) =>
        igrfSerializer.SerializeIgrf(statsBook.Igrf, archive)
            .Then(scoreSheetSerializer.SerializeScoreSheets, statsBook.ScoreSheets)
            .Then(penaltySheetSerializer.SerializePenaltySheets, statsBook.PenaltySheets)
            .Then(lineupSheetSerializer.SerializeLineupSheets, statsBook.LineupSheets);

    private async Task<Result<StatsBook>> Deserialize(ZipArchive archive) =>
        await validator.ValidateStatsBook(archive)
            .Then(async () =>
                (await igrfSerializer.DeserializeIgrf(archive))
                .And(await scoreSheetSerializer.DeserializeScoreSheets(archive))
                .And(Result.Succeed(new PenaltySheetCollection(new("", [], []), new("", [], []))))
                .And(Result.Succeed(new LineupSheetCollection(new("", []), new("", []), new("", []), new("", []))))
                .ThenMap(CreateStatsBook));

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

    private static StatsBook CreateStatsBook((Igrf Igrf, ScoreSheetCollection ScoreSheets, PenaltySheetCollection PenaltySheets, LineupSheetCollection LineupSheets) sheets) =>
        new(sheets.Igrf, sheets.ScoreSheets, sheets.PenaltySheets, sheets.LineupSheets);
}


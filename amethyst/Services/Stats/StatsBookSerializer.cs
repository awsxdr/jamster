using System.IO.Compression;
using amethyst.Domain;
using Func;

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
            .Then(scoreSheetSerializer.SerializeScoreSheets, statsBook.ScoreSheets);

    private async Task<Result<StatsBook>> Deserialize(ZipArchive archive) =>
        await validator.ValidateStatsBook(archive)
            .Then(async () =>
                (await igrfSerializer.DeserializeIgrf(archive))
                .And(await scoreSheetSerializer.DeserializeScoreSheets(archive))
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

    private static StatsBook CreateStatsBook((Igrf Igrf, ScoreSheetCollection ScoreSheets) sheets) =>
        new(sheets.Igrf, sheets.ScoreSheets);
}


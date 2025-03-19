using System.IO.Compression;

namespace amethyst.Services.Stats;

public interface IStatsBookValidator
{
    Task<Result<StatsBookInfo>> ValidateStatsBook(ZipArchive archive);
}

[Singleton]
public class StatsBookValidator(ILogger<StatsBookValidator> logger) : StatsSheetSerializerBase(logger), IStatsBookValidator
{
    private const string IgrfSheetPath = "xl/worksheets/sheet2.xml";

    public Task<Result<StatsBookInfo>> ValidateStatsBook(ZipArchive archive) =>
        GetWorksheetByEntryName(IgrfSheetPath, archive)
            .Then(worksheet =>
            {
                if (GetCellValue(worksheet, 0, 57) == "IGRF Rev. 190101 \u00a9 2019 Women's Flat Track Derby Association (WFTDA)")
                {
                    return Result.Succeed(new StatsBookInfo("WFTDA.190101"));
                }

                return Result<StatsBookInfo>.Fail<InvalidStatsBookError>();
            });
           

    public sealed class InvalidStatsBookError : ResultError;
}

public sealed record StatsBookInfo(string Version);
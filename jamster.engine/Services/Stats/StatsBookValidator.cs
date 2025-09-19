using System.IO.Compression;

namespace jamster.Services.Stats;

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
                var versionCellValue = GetCellValue(worksheet, 0, 57);

                return versionCellValue switch
                {
                    "IGRF Rev. 190101 \u00a9 2019 Women's Flat Track Derby Association (WFTDA)" => 
                        Result.Succeed(new StatsBookInfo("WFTDA.190101")),
                    "IGRF Rev. 20250201 \u00a9 2025 Women's Flat Track Derby Association (WFTDA)" => 
                        Result.Succeed(new StatsBookInfo("WFTDA.20250201")),
                    _ => Result<StatsBookInfo>.Fail<InvalidStatsBookError>()
                };
            });
           

    public sealed class InvalidStatsBookError : ResultError;
}

public sealed record StatsBookInfo(string Version);
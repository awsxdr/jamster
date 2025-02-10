using Func;

namespace amethyst.Services.Stats;

public interface IBlankStatsBookStore
{
    bool BlankStatsBookPresent { get; }
    Task<Result<byte[]>> GetBlankStatsBook();
    Task SetBlankStatsBook(byte[] statsBookData);
}

public class BlankStatsBookStore : IBlankStatsBookStore
{
    public static string BlankStatsBookPath =>
        Path.Combine(RunningEnvironment.RootPath, "files", "blank-statsbook.xlsx");

    public bool BlankStatsBookPresent => File.Exists(BlankStatsBookPath);

    public async Task<Result<byte[]>> GetBlankStatsBook()
    {
        if (!BlankStatsBookPresent)
            return Result<byte[]>.Fail<BlankStatsBookNotConfiguredError>();

        return Result.Succeed(await File.ReadAllBytesAsync(BlankStatsBookPath));
    }

    public async Task SetBlankStatsBook(byte[] statsBookData)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(BlankStatsBookPath)!);

        await File.WriteAllBytesAsync(BlankStatsBookPath, statsBookData);
    }
}
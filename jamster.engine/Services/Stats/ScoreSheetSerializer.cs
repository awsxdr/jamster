using System.IO.Compression;

using jamster.engine.Serialization;

namespace jamster.engine.Services.Stats;

public interface IScoreSheetSerializer
{
    Task<Result<ZipArchive>> SerializeScoreSheets(ScoreSheetCollection scoreSheet, ZipArchive archive);
    Task<Result<ScoreSheetCollection>> DeserializeScoreSheets(ZipArchive archive);
}

[Singleton]
public class ScoreSheetSerializer(ILogger<ScoreSheetSerializer> logger) : StatsSheetSerializerBase(logger), IScoreSheetSerializer
{
    private const string ScoreSheetName = "Score";

    public Task<Result<ZipArchive>> SerializeScoreSheets(ScoreSheetCollection scoreSheets, ZipArchive archive) =>
        GetWorksheet(ScoreSheetName, archive)
            .Then(WriteScoreSheets, scoreSheets)
            .Then(UpdateSharedStrings, archive)
            .Then(WriteWorksheet)
            .Then(() => Result.Succeed(archive));

    public Task<Result<ScoreSheetCollection>> DeserializeScoreSheets(ZipArchive archive) =>
        GetWorksheet(ScoreSheetName, archive)
            .ThenMap(worksheet =>
                new ScoreSheetCollection(
                    ReadScoreSheet(0, 1, worksheet),
                    ReadScoreSheet(19, 1, worksheet),
                    ReadScoreSheet(0, 43, worksheet),
                    ReadScoreSheet(19, 43, worksheet)
                ));

    private static Result<Worksheet> WriteScoreSheets(ScoreSheetCollection scoreSheets, Worksheet worksheet) =>
        WriteScoreSheet(scoreSheets.HomePeriod1, 0, 1, worksheet)
            .Then(WriteScoreSheet, scoreSheets.AwayPeriod1, 19, 1)
            .Then(WriteScoreSheet, scoreSheets.HomePeriod2, 0, 43)
            .Then(WriteScoreSheet, scoreSheets.AwayPeriod2, 19, 43);

    private static ScoreSheet ReadScoreSheet(int column, int row, Worksheet worksheet) =>
        new(
            GetCellValue(worksheet, column + 11, row),
            GetCellValue(worksheet, column + 14, row),
            ReadScores(column, row + 3, worksheet)
        );

    private static Result<Worksheet> WriteScoreSheet(ScoreSheet scoreSheet, int column, int row, Worksheet worksheet) =>
        SetCellValue(column + 11, row, scoreSheet.ScoreKeeper, worksheet)
            .Then(SetCellValue, column + 14, row, scoreSheet.JammerRef)
            .Then(WriteScores, column, row + 3, scoreSheet.Lines);

    private static ScoreSheetLine[] ReadScores(int column, int row, Worksheet worksheet) =>
        Enumerable.Range(0, 38)
            .Select(i => ReadScoreLine(column, row + i, worksheet))
            .ToArray();

    private static Result<Worksheet> WriteScores(int column, int row, ScoreSheetLine[] lines, Worksheet worksheet) =>
        lines.Aggregate(
                (Row: row, Result: Result.Succeed(worksheet)),
                (result, line) => (result.Row + 1, result.Result.Then(WriteScoreLine, column, result.Row, line)))
            .Result;

    private static ScoreSheetLine ReadScoreLine(int column, int row, Worksheet worksheet) =>
        new(
            GetCellValue(worksheet, column, row).Map(s => int.TryParse(s, out var i) ? (Union<int, string>)i : s),
            GetCellValue(worksheet, column + 1, row),
            GetCellValue(worksheet, column + 2, row).Trim().ToLowerInvariant() == "x",
            GetCellValue(worksheet, column + 3, row).Trim().ToLowerInvariant() == "x",
            GetCellValue(worksheet, column + 4, row).Trim().ToLowerInvariant() == "x",
            GetCellValue(worksheet, column + 5, row).Trim().ToLowerInvariant() == "x",
            GetCellValue(worksheet, column + 6, row).Trim().ToLowerInvariant() == "x",
            ReadTrips(column + 7, row, worksheet)
        );

    private static Result<Worksheet> WriteScoreLine(int column, int row, ScoreSheetLine line, Worksheet worksheet) =>
        (
            line.Jam.Is<string>(out var s) ? SetCellValue(column, row, s, worksheet)
            : line.Jam.Is<int>(out var i) ? SetCellValue(column, row, i, worksheet)
            : Result<Worksheet>.Fail<UnexpectedJamNumberTypeError>()
        )
            .Then(SetCellValue, column + 1, row, line.JammerNumber)
            .Then(SetCellValue, column + 2, row, line.Lost ? "X" : null)
            .Then(SetCellValue, column + 3, row, line.Lead ? "X" : null)
            .Then(SetCellValue, column + 4, row, line.Call ? "X" : null)
            .Then(SetCellValue, column + 5, row, line.Injury ? "X" : null)
            .Then(SetCellValue, column + 6, row, line.NoInitial ? "X" : null)
            .Then(WriteTrips, column + 7, row, line.Trips);

    private static ScoreSheetTrip[] ReadTrips(int column, int row, Worksheet worksheet) =>
        Enumerable.Range(0, 9)
            .Select(i => int.TryParse(GetCellValue(worksheet, column + i, row), out var score) ? score : (int?)null)
            .Select(s => new ScoreSheetTrip(s))
            .ToArray();

    private static Result<Worksheet> WriteTrips(int column, int row, ScoreSheetTrip[] trips, Worksheet worksheet) =>
        trips.Aggregate(
            (Column: column, Result: Result.Succeed(worksheet)),
            (result, trip) => (
                result.Column + 1, 
                result.Result.Then(SetCellValue, result.Column, row, trip.Score))
        ).Result;

    public sealed class UnexpectedJamNumberTypeError : ResultError;
}
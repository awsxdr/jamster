using System.IO.Compression;

using jamster.engine.Serialization;

namespace jamster.engine.Services.Stats;

public interface ILineupSheetSerializer
{
    Task<Result<ZipArchive>> SerializeLineupSheets(LineupSheetCollection lineupSheets, ZipArchive archive);
}

[Singleton]
public class LineupSheetSerializer(ILogger<LineupSheetSerializer> logger) : StatsSheetSerializerBase(logger), ILineupSheetSerializer
{
    private const string LineupSheetName = "Lineups";

    public Task<Result<ZipArchive>> SerializeLineupSheets(LineupSheetCollection lineupSheets, ZipArchive archive) =>
        GetWorksheet(LineupSheetName, archive)
            .Then(WriteLineupSheets, lineupSheets)
            .Then(UpdateSharedStrings, archive)
            .Then(WriteWorksheet)
            .Then(() => Result.Succeed(archive));

    private static Result<Worksheet> WriteLineupSheets(LineupSheetCollection lineupSheets, Worksheet worksheet) =>
        WriteLineupSheet(lineupSheets.HomePeriod1, 0, 1, worksheet)
        .Then(WriteLineupSheet, lineupSheets.AwayPeriod1, 26, 1)
        .Then(WriteLineupSheet, lineupSheets.HomePeriod2, 0, 43)
        .Then(WriteLineupSheet, lineupSheets.AwayPeriod2, 26, 43);

    private static Result<Worksheet> WriteLineupSheet(LineupSheet lineupSheet, int column, int row, Worksheet worksheet) =>
        SetCellValue(column + 15, row, lineupSheet.LineupTracker, worksheet)
            .Then(WriteLineups, column, row + 3, lineupSheet.Lines);

    private static Result<Worksheet> WriteLineups(int column, int row, LineupSheetLine[] lines, Worksheet worksheet) =>
        lines.Aggregate(
                (Row: row, Result: Result.Succeed(worksheet)),
                (result, line) => (result.Row + 1, result.Result.Then(WriteLineup, column, result.Row, line)))
            .Result;

    private static Result<Worksheet> WriteLineup(int column, int row, LineupSheetLine line, Worksheet worksheet) =>
        SetCellValue(column + 1, row, line.NoPivot ? "X" : null, worksheet)
        .Then(WriteSkaters, column + 2, row, line.Skaters);

    private static Result<Worksheet> WriteSkaters(int column, int row, LineupSkater[] skaters, Worksheet worksheet) =>
        skaters.Aggregate(
            (Column: column, Result: Result.Succeed(worksheet), Jammer: true),
            (result, skater) => (
                result.Column + 4,
                result.Result
                    .Then(SetCellValue, result.Column, row, result.Jammer ? null : skater.Number)
                    .Then(SetCellValue, result.Column + 1, row, skater.BoxSymbols[0])
                    .Then(SetCellValue, result.Column + 2, row, skater.BoxSymbols[1])
                    .Then(SetCellValue, result.Column + 3, row, skater.BoxSymbols[2])
                ,
                false)
        ).Result;

    public sealed class UnexpectedJamNumberTypeError : ResultError;
}
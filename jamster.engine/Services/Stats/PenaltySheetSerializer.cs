using System.IO.Compression;
using jamster.Serialization;

namespace jamster.Services.Stats;

public interface IPenaltySheetSerializer
{
    Task<Result<ZipArchive>> SerializePenaltySheets(PenaltySheetCollection penaltySheets, ZipArchive archive);
}

[Singleton]
public class PenaltySheetSerializer(ILogger<PenaltySheetSerializer> logger) : StatsSheetSerializerBase(logger), IPenaltySheetSerializer
{
    private const string PenaltySheetName = "Penalties";

    public Task<Result<ZipArchive>> SerializePenaltySheets(PenaltySheetCollection penaltySheets, ZipArchive archive) =>
        GetWorksheet(PenaltySheetName, archive)
            .Then(WritePenaltySheets, penaltySheets)
            .Then(UpdateSharedStrings, archive)
            .Then(WriteWorksheet)
            .Then(() => Result.Succeed(archive));

    private static Result<Worksheet> WritePenaltySheets(PenaltySheetCollection penaltySheets, Worksheet worksheet) =>
        WritePenaltySheet(penaltySheets.Period1, 0, worksheet)
            .Then(WritePenaltySheet, penaltySheets.Period2, 28);

    private static Result<Worksheet> WritePenaltySheet(PenaltySheet penaltySheet, int column, Worksheet worksheet) =>
        SetCellValue(column + 13, 1, penaltySheet.PenaltyTracker, worksheet)
            .Then(WritePenaltyLines, column + 1, penaltySheet.HomePenalties)
            .Then(WritePenaltyLines, column + 16, penaltySheet.AwayPenalties);

    private static Result<Worksheet> WritePenaltyLines(int column, PenaltySheetLine[] lines, Worksheet worksheet) =>
        lines.Aggregate(
                (Row: 4, Result: Result.Succeed(worksheet)),
                (result, line) => (result.Row + 2, result.Result.Then(WritePenalties, column, result.Row, line)))
            .Result;

    private static Result<Worksheet> WritePenalties(int column, int row, PenaltySheetLine line, Worksheet worksheet) =>
        line.Penalties.Aggregate(
                (Column: column + line.Offset, Result: Result.Succeed(worksheet)),
                (result, penalty) => (result.Column + 1, result.Result.Then(WritePenalty, result.Column, row, penalty)))
            .Result
            .Then(WriteExpulsionOrFoulout, column + 9, row, line);

    private static Result<Worksheet> WritePenalty(int column, int row, Penalty penalty, Worksheet worksheet) =>
        SetCellValue(column, row, penalty.Code, worksheet)
            .Then(SetCellValue, column, row + 1, penalty.JamNumber);

    private static Result<Worksheet> WriteExpulsionOrFoulout(int column, int row, PenaltySheetLine line, Worksheet worksheet) =>
        line.Expulsion is not null ? WritePenalty(column, row, line.Expulsion, worksheet)
        : line.Penalties.Length + line.Offset >= 7 ? WritePenalty(column, row, new Penalty(line.Penalties[6 - line.Offset].JamNumber, "FO"), worksheet)
        : Result.Succeed(worksheet);
}
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Func;

namespace amethyst.Services.Stats;

public abstract class StatsSheetSerializerBase(ILogger logger)
{
    protected Task<Result<sst>> GetSharedStrings(ZipArchive archive) =>
        GetEntry(archive, "xl/sharedStrings.xml")
            .Then(LoadDocumentFromEntry)
            .Then(document =>
            {
                if (document.Root is null)
                {
                    logger.LogWarning("Document uploaded did non contain a valid sharedStrings.xml");
                    return Result<sst>.Fail<InvalidStatsBookFileFormatError>();
                }

                var sharedStrings = (sst)new XmlSerializer(typeof(sst)).Deserialize(document.CreateReader())!;

                return Result.Succeed(sharedStrings);
            });


    protected Task<Result<Worksheet>> UpdateSharedStrings(ZipArchive archive, Worksheet sheet) =>
        GetEntry(archive, "xl/sharedStrings.xml")
            .Then(entry =>
            {
                sheet.SharedStrings.uniqueCount = sheet.SharedStrings.si.Length;

                using var outputStream = new MemoryStream();
                new XmlSerializer(typeof(sst)).Serialize(outputStream, sheet.SharedStrings);

                outputStream.Position = 0;
                var document = XDocument.Load(outputStream);

                return WriteDocumentToEntry(entry, document).Then(() => Result.Succeed(sheet));
            });

    protected Result<ZipArchiveEntry> GetEntry(ZipArchive archive, string path)
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

    protected async Task<Result<XDocument>> LoadDocumentFromEntry(ZipArchiveEntry entry)
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

    protected Task<Result<Worksheet>> GetWorksheetByEntryName(string entryName, ZipArchive archive) =>
        GetEntry(archive, entryName)
            .Then(entry =>
                LoadDocumentFromEntry(entry)
                    .Then(document =>
                        GetSharedStrings(archive).ThenMap(sharedStrings => new Worksheet(entry, document, sharedStrings))));

    protected static async Task<Result> WriteDocumentToEntry(ZipArchiveEntry entry, XDocument document)
    {
        await using var entryStream = entry.Open();
        entryStream.SetLength(0);

        await document.SaveAsync(entryStream, SaveOptions.DisableFormatting, default);

        return Result.Succeed();
    }

    protected static async Task<Result> WriteWorksheet(Worksheet worksheet)
    {
        await using var entryStream = worksheet.Entry.Open();
        entryStream.SetLength(0);

        await worksheet.Document.SaveAsync(entryStream, SaveOptions.DisableFormatting, default);

        return Result.Succeed();
    }

    protected static Result<(XElement Cell, XNamespace Namespace)> GetWriteableCell(int column, int row, Worksheet sheet)
    {
        if (sheet.Document.Root is null)
            return Result<(XElement, XNamespace)>.Fail<InvalidStatsBookFileFormatError>();

        var root = sheet.Document.Root;
        var @namespace = root.Name.Namespace;

        var rowString = row.ToString();
        var columnString = GetColumnString(column);

        var sheetData = root.Element(@namespace + "sheetData");

        if (sheetData == null)
            return Result<(XElement, XNamespace)>.Fail<InvalidStatsBookFileFormatError>();

        var documentRow =
            sheetData.Elements(@namespace + "row").SingleOrDefault(e => e.Attribute("r")?.Value == rowString)
            ?? new XElement(@namespace + "row", new XAttribute("r", rowString)).Tee(sheetData.Add);

        var cellString = $"{columnString}{rowString}";

        var documentCell =
            documentRow.Elements(@namespace + "c").SingleOrDefault(e => e.Attribute("r")?.Value == cellString)
            ?? new XElement(@namespace + "c", new XAttribute("r", cellString)).Tee(documentRow.Add);

        return Result.Succeed((documentCell, @namespace));
    }

    protected static Result<Worksheet> SetCellValue(int column, int row, double value, Worksheet sheet) =>
        GetWriteableCell(column, row, sheet)
            .Then(x =>
            {
                var (cell, @namespace) = x;
                cell.Attribute("t")?.Remove();
                cell.Add(new XElement(@namespace + "v", value));

                return Result.Succeed(sheet);
            });

    protected static Result<Worksheet> SetCellValue(int column, int row, int value, Worksheet sheet) =>
        SetCellValue(column, row, (int?)value, sheet);

    protected static Result<Worksheet> SetCellValue(int column, int row, int? value, Worksheet sheet) =>
        GetWriteableCell(column, row, sheet)
            .Then(x =>
            {
                var (cell, @namespace) = x;

                if(value != null)
                    cell.Add(new XElement(@namespace + "v", value));

                return Result.Succeed(sheet);
            });

    protected static Result<Worksheet> SetCellValue(int column, int row, string? value, Worksheet sheet) =>
        GetWriteableCell(column, row, sheet)
            .Then(x =>
            {
                var (cell, @namespace) = x;
                cell.Attribute("t")?.Remove();

                if (value != null)
                {
                    cell.Add(new XAttribute("t", "s"));

                    var sharedStringIndex = sheet.SharedStrings.si.Length;
                    cell.Add(new XElement(@namespace + "v", sharedStringIndex));

                    sheet.SharedStrings.si = sheet.SharedStrings.si.Append(new() { t = new() { Value = value } }).ToArray();
                }

                return Result.Succeed(sheet);
            });

    protected static string GetCellValue(Worksheet document, int column, int row)
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
            int.TryParse(reference, out var index) && index < document.SharedStrings.si.Length
                ? document.SharedStrings.si[index].t.Value
                : string.Empty;
    }

    protected static string GetColumnString(int column) =>
        column >= 26
        ? $"{(char)('A' + column / 26 - 1)}{(char)('A' + column % 26)}"
        : ((char)('A' + column)).ToString();


    protected sealed record Worksheet(ZipArchiveEntry Entry, XDocument Document, sst SharedStrings);
}

public sealed class InvalidStatsBookFileFormatError : ResultError;
public sealed class BlankStatsBookNotConfiguredError : ResultError;

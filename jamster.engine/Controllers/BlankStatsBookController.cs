using jamster.engine.Domain;
using jamster.engine.Services.Stats;

using Microsoft.AspNetCore.Mvc;

using static jamster.engine.Services.Stats.StatsBookValidator;

namespace jamster.engine.Controllers;

[ApiController, Route("api/blankStatsBook")]
public class BlankStatsBookController(IBlankStatsBookStore blankStatsBookStore, IStatsBookSerializer statsBookSerializer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetBlankStatsBook() =>
        await blankStatsBookStore.GetBlankStatsBook() switch
        {
            Success<byte[]> s => File(s.Value, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            Failure<BlankStatsBookNotConfiguredError> => NotFound(),
            var r => throw new UnexpectedResultException(r)
        };

    [HttpHead]
    public IActionResult GetBlankStatsBookResponse() =>
        blankStatsBookStore.BlankStatsBookPresent
            ? Ok()
            : NotFound();

    [HttpPost]
    public async Task<IActionResult> SetBlankStatsBook(IFormFile file)
    {
        using var fileData = new MemoryStream();
        await file.CopyToAsync(fileData);

        return await statsBookSerializer.ValidateStream(fileData)
                .ThenMap(async info =>
                {
                    fileData.Position = 0;
                    await blankStatsBookStore.SetBlankStatsBook(fileData.ToArray());
                    return info;
                }) switch
            {
                Success<StatsBookInfo> s => Ok(s),
                Failure<InvalidStatsBookError> or Failure<InvalidStatsBookFileFormatError> => BadRequest(),
                var r => throw new UnexpectedResultException(r)
            };
    }
}
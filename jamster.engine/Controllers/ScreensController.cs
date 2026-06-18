using jamster.engine.Domain;
using jamster.engine.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace jamster.engine.Controllers;

[ApiController, Route("/api/v1/screens")]
public class ScreensController(ICustomScreensService customScreensService, IContentTypeProvider contentTypeProvider) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> GetCustomScreens() =>
        Ok(
            (await customScreensService.GetCustomScreens())
            .Select(s => new CustomScreenModel(s.Id, s.Name, s.Category, s.OwnTab, $"/api/v1/screens/{s.Id}"))
        );

    [HttpGet("{screenId:guid}")]
    public async Task<IActionResult> GetScreenIndex(Guid screenId) =>
        await customScreensService.GetScreenFileLocation(screenId, "index.html") is Success
            ? RedirectToAction(nameof(GetScreenFile), new { screenId, filePath = "index.html" })
            : RedirectToAction(nameof(GetScreenFile), new { screenId, filePath = "index.htm" });

    [HttpGet("{screenId:guid}/{**filePath}")]
    public async Task<IActionResult> GetScreenFile(Guid screenId, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            RedirectToAction(nameof(GetScreenIndex));

        return await customScreensService.GetScreenFileLocation(screenId, filePath) switch
        {
            Success<CustomScreenPath> s => s.Value switch
            {
                CustomScreenFilePath f => PhysicalFile(f.FilePath, contentTypeProvider.TryGetContentType(f.FilePath, out var contentType) ? contentType : "application/octet-stream"),
                CustomScreenUrlPath u => Redirect(u.Url),
                _ => throw new CustomScreensService.PathTypeNotSupportedException()
            },
            Failure<NotFoundError> => NotFound(),
            Failure<CustomScreensService.PathTraversalDetectedError> => BadRequest(),
            var r => throw new UnexpectedResultException(r)
        };
    }

    public record CustomScreenModel(Guid Id, string Name, string Category, bool OwnTab, string Url);
}
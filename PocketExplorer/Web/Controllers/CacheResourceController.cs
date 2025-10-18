using Microsoft.AspNetCore.Mvc;

namespace PocketExplorer.Web.Controllers;

[Route("cache")]
[ApiController]
public class CacheResourceController(DataKeeper dataKeeper) : ControllerBase
{
    private readonly DataKeeper _dataKeeper =  dataKeeper;

    [HttpGet("thumbnail/{thumbnailId}")]
    public Task<IActionResult> GetThumbnail(string thumbnailId)
    {
        if (!_dataKeeper.ThumbnailDict.TryGetValue(thumbnailId, out var thumbnail))
        {
            return Task.FromResult<IActionResult>(NotFound());
        }
        return Task.FromResult<IActionResult>(File(thumbnail, "image/jpeg"));
    }
}
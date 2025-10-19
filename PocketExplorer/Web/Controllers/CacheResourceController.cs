using Microsoft.AspNetCore.Mvc;

namespace PocketExplorer.Web.Controllers;

[Route("cache")]
[ApiController]
public class CacheResourceController(DataKeeper dataKeeper) : ControllerBase
{
    private readonly DataKeeper _dataKeeper =  dataKeeper;

    [HttpGet("thumbnail/{thumbnailId}")]
    public IActionResult GetThumbnail(string thumbnailId)
    {
        if (!_dataKeeper.ThumbnailDict.TryGetValue(thumbnailId, out var thumbnail))
        {
            return NotFound();
        }
        return File(thumbnail, "image/jpeg");
    }
}
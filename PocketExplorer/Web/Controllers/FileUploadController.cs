using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using PocketExplorer.Data;
using PocketExplorer.Web.Attr;
using PocketExplorer.Web.Utils;
using System.Net;
using SFile = System.IO.File;

namespace PocketExplorer.Web.Controllers;

[Route("api/upload")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private List<PeLocation> _peLocations;

    public FileUploadController(DataKeeper dataKeeper)
    {
        _peLocations = [.. dataKeeper.PeInstance.Locations];
    }

    [HttpPost("~/api/upload/{locationName}/{*path}")]
    [DisableFormValueModelBinding]
    public async Task<IActionResult> UploadFile(string locationName, string path = "")
    {
        var request = HttpContext.Request;
        var locationRoot = _peLocations.SingleOrDefault(lc => lc.Name == locationName)?.Path;
        if (locationRoot is null)
        {
            return NotFound();
        }
        var uploadDir = FileHelper.GetFullPath(locationRoot, path);
        if (uploadDir is null)
        {
            return NotFound();
        }

        if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
        {
            return BadRequest();
        }

        var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), new FormOptions().MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, request.Body);

        for (var section = await reader.ReadNextSectionAsync();
            section is not null;
            section = await reader.ReadNextSectionAsync())
        {
            if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
            {
                if (contentDisposition.IsFileDisposition())
                {
                    var fileName = WebUtility.HtmlEncode(contentDisposition.FileName.Value) ?? throw new NullReferenceException();
                    using var targetStream = SFile.Create(Path.Combine(uploadDir, fileName));
                    await section.Body.CopyToAsync(targetStream);
                }
            }
        }

        return Ok();
    }

}

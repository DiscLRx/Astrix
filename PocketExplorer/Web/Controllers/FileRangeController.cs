using Microsoft.AspNetCore.Mvc;
using PocketExplorer.Data;
using PocketExplorer.Web.Utils;
using SFile = System.IO.File;

namespace PocketExplorer.Web.Controllers;

[Route("file")]
[ApiController]
public class FileRangeController : ControllerBase
{
    private int _statusCode = 206;

    private List<PeLocation> _locations;

    public FileRangeController(DataKeeper dataKeeper)
    {
        _locations = [.. dataKeeper.PeInstance.Locations];
    }

    [HttpGet("~/file/{locationName}/{*filePath}")]
    public async Task GetFileRange(string filePath, string locationName)
    {
        var locationRoot = _locations.Single(lc => lc.Name == locationName).Path;

        var fileFullPath = FileHelper.GetFullPath(locationRoot, filePath);
        if (fileFullPath == null)
        {
            return;
        }

        using var fs = SFile.OpenRead(fileFullPath);
        long begin, end;
        try
        {
            ParseRangeHeader(in fs, out begin, out end);
        }
        catch
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        var response = HttpContext.Response;
        response.StatusCode = _statusCode;
        response.Headers.AcceptRanges = "bytes";
        response.ContentType = MimeMapper.GetMimeType(fileFullPath);
        response.ContentLength = end - begin + 1;
        response.Headers.ContentRange = $"bytes {begin}-{end}/{fs.Length}";

        await WriteRangeToResponse(fs, begin, end);
    }

    private void ParseRangeHeader(in FileStream stream, out long begin, out long end)
    {
        var range = HttpContext.Request.Headers.Range;
        if (range.Count == 0)
        {
            _statusCode = 200;
            begin = 0;
            end = stream.Length - 1;
        }
        else if (range.Count == 1)
        {
            var r = range[0]!.Split("=")[1].Split("-");
            begin = Convert.ToInt64(r[0]);
            if (begin > stream.Length || begin < 0)
            {
                throw new ArgumentOutOfRangeException($"position {begin} is out of the range 0-{stream.Length}");
            }
            if (!string.IsNullOrWhiteSpace(r[1]))
            {
                end = Convert.ToInt64(r[1]);
                if (end > stream.Length - 1 || end < begin)
                {
                    throw new ArgumentOutOfRangeException($"position {end} is out of the range 0-{stream.Length}");
                }
            }
            else
            {
                end = stream.Length - 1;
            }
        }
        else
        {
            throw new ArgumentException("Invalid range header");
        }
    }

    private async Task WriteRangeToResponse(FileStream fs, long begin, long end)
    {
        fs.Position = begin;
        long totalSize = end - begin + 1;
        int bufferSize = 1024 * 1024;
        var buffer = new byte[bufferSize];
        long currentPosition = 0;
        var body = HttpContext.Response.Body;
        while (currentPosition < totalSize)
        {
            int readSize = (int)Math.Min(bufferSize, totalSize - currentPosition);
            await fs.ReadAsync(buffer.AsMemory(0, readSize));
            await body.WriteAsync(buffer.AsMemory(0, readSize));
            currentPosition += readSize;
        }
    }

}

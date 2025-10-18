using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuGet.Packaging;
using PocketExplorer.Data;
using PocketExplorer.Web.Controllers;
using PocketExplorer.Web.Utils;

namespace PocketExplorer.Web.Pages;

public class FileBrowseModel(DataKeeper dataKeeper) : PageModel
{
    public List<DirectoryItem> DirectoryItems = [];

    private List<PeLocation> _peLocations = [.. dataKeeper.PeInstance.Locations];
    private Dictionary<string, byte[]> _thumbnail = dataKeeper.ThumbnailDict;

    public string? LocationName { get; set; }
    public string? Path { get; set; }

    public IActionResult OnGet(string locationName, string path)
    {
        path ??= "";

        LocationName = locationName;
        Path = path;

        var locationRoot = _peLocations.SingleOrDefault(lc => lc.Name == locationName)?.Path;

        if (locationRoot == null)
        {
            return NotFound();
        }

        var (items, thumbnailDict) = FileHelper.GetDirItems(locationRoot, path);
        if (items is null)
        {
            return NotFound();
        }

        _thumbnail.Clear();
        _thumbnail.AddRange(thumbnailDict);

        items.Sort();

        DirectoryItems = items;
        return Page();
    }
}
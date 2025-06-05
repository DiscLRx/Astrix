using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketExplorer.Data;
using PocketExplorer.Web.Utils;

namespace PocketExplorer.Web.Pages;

public class FileBrowseModel : PageModel
{
    public List<DirectoryItem> DirectoryItems = [];

    private List<PeLocation> _peLocations { get; set; }

    public string? LocationName { get; set; }
    public string? Path { get; set; }

    public FileBrowseModel(DataKeeper dataKeeper)
    {
        _peLocations = [.. dataKeeper.PeInstance.Locations];
    }

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

        var items = FileHelper.GetDirItems(locationRoot, path);
        if (items is null)
        {
            return NotFound();
        }
        DirectoryItems = items;
        return Page();
    }

}

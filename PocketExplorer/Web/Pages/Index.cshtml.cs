using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketExplorer.Data;

namespace PocketExplorer.Web.Pages
{
    public class IndexModel : PageModel
    {
        public List<PeLocation> Locations { get; set; }

        public IndexModel(DataKeeper dataKeeper)
        {
            Locations = [.. dataKeeper.PeInstance.Locations];
        }

        public void OnGet()
        {

        }
    }
}

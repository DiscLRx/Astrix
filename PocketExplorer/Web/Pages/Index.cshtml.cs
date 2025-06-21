using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketExplorer.Data;

namespace PocketExplorer.Web.Pages
{
    public class IndexModel : PageModel
    {
        public int Port { get; set; }
        public List<PeLocation> Locations { get; set; }

        public IndexModel(DataKeeper dataKeeper)
        {
            Port = dataKeeper.PeInstance.Port;
            Locations = [.. dataKeeper.PeInstance.Locations];
        }
    }
}

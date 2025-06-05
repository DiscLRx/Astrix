using PocketExplorer.Data;

namespace PocketExplorer.Web;

public class DataKeeper()
{
    public PeInstance PeInstance
    {
        get => field ?? throw new NullReferenceException("PeInstance is null");
        set;
    }
}

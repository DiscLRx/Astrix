using System.Windows;

namespace AstrixUI;

public partial class App : Application
{
    private Mutex _mutex;

    public App()
    {
        _mutex = new Mutex(true, "AstrixInstanceMutex", out bool isUniqueInstance);
        if (!isUniqueInstance)
        {
            Shutdown();
        }
    }
}

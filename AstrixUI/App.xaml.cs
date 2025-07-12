using System.IO;
using System.IO.Pipes;
using System.Windows;

namespace AstrixUI;

public partial class App : Application
{
    private readonly Mutex _mutex;

    public App()
    {
        _mutex = new Mutex(true, "AstrixInstanceMutex", out bool isUniqueInstance);
        if (!isUniqueInstance)
        {
            using var pipeClient = new NamedPipeClientStream(".", "AstrixAwakePipe", PipeDirection.Out, PipeOptions.None);
            using var sw = new StreamWriter(pipeClient);
            pipeClient.Connect(TimeSpan.FromSeconds(3));
            Shutdown();
        }
    }
}

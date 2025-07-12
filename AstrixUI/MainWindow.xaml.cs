using AstrixUI.Pages;
using HandyControl.Controls;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HcTabControl = HandyControl.Controls.TabControl;
using HcTabItem = HandyControl.Controls.TabItem;
using HcWindow = HandyControl.Controls.Window;

namespace AstrixUI;

public class WindowStatus() : INotifyPropertyChanged
{
    private bool _isWindowTopMost = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsWindowTopMost
    {
        get => _isWindowTopMost;
        set
        {
            _isWindowTopMost = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWindowTopMost)));
        }
    }
}

public partial class MainWindow : HcWindow
{
    private static ImageSource _icon = Imaging.CreateBitmapSourceFromHIcon(new Icon($"{AppDomain.CurrentDomain.BaseDirectory}/Assets/Astrix.ico").Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

    private NotifyIcon _notifyIcon = new()
    {
        Icon = _icon,
        Text = "Astrix",
        Visibility = Visibility.Visible
    };

    private Frame _pocketExplorerFrame = new()
    {
        BorderThickness = new Thickness(0),
        Content = new PocketExplorerUI()
    };

    public WindowStatus WindowStatus = new();

    public MainWindow()
    {
        new AstrixAwakePipeServer(this).Run();

        Icon = _icon;
        InitializeComponent();

        _notifyIcon.Init();
        _notifyIcon.Click += ShowWindow;
        SetNotifyIconMenu();

        contentControl.Content = _pocketExplorerFrame;

        topMostBtn.DataContext = WindowStatus;
    }

    private void SetNotifyIconMenu()
    {
        var exitMenuItem = new MenuItem()
        {
            Header = "Exit",
            Width = 80,
            Padding = default
        };
        exitMenuItem.Click += (s, e) =>
        {
            Application.Current.Shutdown();
        };
        var contextMenu = new ContextMenu()
        {
            ItemsSource = new MenuItem[]
            {
                exitMenuItem
            }
        };
        _notifyIcon.ContextMenu = contextMenu;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        Hide();
        e.Cancel = true;
    }

    private void ShowWindow(object sender, RoutedEventArgs e)
    {
        Show();
        Activate();
    }

    private void ToggleTopMost(object sender, RoutedEventArgs e)
    {
        WindowStatus.IsWindowTopMost = !WindowStatus.IsWindowTopMost;
        Topmost = WindowStatus.IsWindowTopMost;
    }

    private void MenuTabChanged(object sender, SelectionChangedEventArgs e)
    {
        var menuTab = sender as HcTabControl;
        var selectedTab = menuTab?.SelectedItem as HcTabItem;
        if (contentControl is null)
        {
            return;
        }
        contentControl.Content = selectedTab?.Name switch
        {
            "pe" => _pocketExplorerFrame,
            _ => throw new NotImplementedException()
        };
    }

    private void OpenSetting(object sender, RoutedEventArgs e)
    {
        var settingWindow = new SettingWindow();
        settingWindow.Owner = this;
        settingWindow.ShowDialog();
    }
}

public class AstrixAwakePipeServer
{
    private readonly MainWindow _mw;

    public AstrixAwakePipeServer(MainWindow mw)
    {
        _mw = mw;
    }

    public void Run()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                using var server = new NamedPipeServerStream("AstrixAwakePipe", PipeDirection.In, 1);
                await server.WaitForConnectionAsync();
                ShowWindow();
            }
        });
    }

    private void ShowWindow()
    {
        _mw.Dispatcher.Invoke(() =>
        {
            _mw.Show();
            _mw.Activate();
        });
    }


}
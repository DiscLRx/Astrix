using AstrixUI.Pages;
using HandyControl.Controls;
using HandyControl.Interactivity;
using System.ComponentModel;
using System.Drawing;
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
    private static ImageSource _icon = Imaging.CreateBitmapSourceFromHIcon(new Icon(@"Assets/Astrix.ico").Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

    private NotifyIcon _notifyIcon = new()
    {
        Icon = _icon,
        Text = "Astrix",
        Visibility = Visibility.Visible
    };

    private Frame _pocketExplorerFrame = new()
    {
        BorderThickness = new System.Windows.Thickness(0),
        Content = new PocketExplorerUI()
    };

    public WindowStatus WindowStatus = new();
    public bool IsWindowTopMost { get; set; }
    public MainWindow()
    {
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
            Width = 80
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

    private void ToggleTopMost(object sender, System.Windows.RoutedEventArgs e)
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
}
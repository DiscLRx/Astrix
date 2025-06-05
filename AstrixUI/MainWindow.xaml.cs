using AstrixUI.Pages;
using System.ComponentModel;
using System.Windows.Controls;
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
    private Frame _pocketExplorerFrame = new()
    {
        BorderThickness = new System.Windows.Thickness(0),
        Content = new PocketExplorerUI()
    };

    public WindowStatus WindowStatus = new();
    public bool IsWindowTopMost { get; set; }
    public MainWindow()
    {
        InitializeComponent();

        contentControl.Content = _pocketExplorerFrame;

        topMostBtn.DataContext = WindowStatus;
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
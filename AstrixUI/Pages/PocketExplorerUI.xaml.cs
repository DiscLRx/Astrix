using Microsoft.Win32;
using PocketExplorer;
using PocketExplorer.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HcMessageBox = HandyControl.Controls.MessageBox;
namespace AstrixUI.Pages;

public class CommonCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Action<object?> _action;
    public CommonCommand(Action<object?> action)
    {
        _action = action;
    }
    public bool CanExecute(object? _) => true;

    public void Execute(object? parameter) => _action(parameter);
}

public class PocketExplorerViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
    public PeControl _peControl { get; }

    public PeInstance CurrentInstance
    {
        get
        {
            var a = field;
            return a;
        }
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public List<int> Ports
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    #region Parameters

    public string LocationNameText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;
    public string LocationPathText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;
    public string InstancePortText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    #endregion

    #region Commands

    public ICommand OpenDirectoryCmd { get; set; }

    public ICommand SelectDirectoryCmd { get; set; }

    public ICommand AddLocationCmd { get; set; }

    public ICommand RemoveLocationCmd { get; set; }

    public ICommand SelectedPortChangeCmd { get; set; }

    public ICommand AddInstanceCmd { get; set; }

    public ICommand RemoveInstanceCmd { get; set; }

    public ICommand SetInstanceEnableCmd { get; set; }

    public ICommand SwitchAccessLock { get; set; }

    #endregion

    public void UpdatePorts() => Ports = _peControl.GetPorts().ToList();

    public PocketExplorerViewModel()
    {
        _peControl = new();
        UpdatePorts();
        if (Ports.Count == 0)
        {
            _peControl.AddInstance(80);
        }
        UpdatePorts();
        CurrentInstance = _peControl.GetInstance(Ports.First());

        OpenDirectoryCmd = new CommonCommand((parm) =>
        {
            var name = (parm as string)!;
            var locationInfo = CurrentInstance.Locations.Single(lc => lc.Name == name);
            Process.Start("explorer.exe", locationInfo.Path);
        });

        SelectDirectoryCmd = new CommonCommand((_) =>
        {
            var openFolderDialog = new OpenFolderDialog()
            {
                RootDirectory = "~",
                Multiselect = false
            };
            openFolderDialog.ShowDialog();
            LocationPathText = openFolderDialog.FolderName;
        });

        AddLocationCmd = new CommonCommand((_) =>
        {
            LocationNameText = LocationNameText.Trim();
            LocationPathText = LocationPathText.Trim();
            if (string.IsNullOrWhiteSpace(LocationNameText) || string.IsNullOrWhiteSpace(LocationPathText))
            {
                return;
            }
            var port = CurrentInstance.Port;
            if (!_peControl.IsLocationNameUnique(port, LocationNameText))
            {
                return;
            }
            var reg = new Regex(@"/|\\");
            if (reg.IsMatch(LocationNameText))
            {
                return;
            }
            _peControl.AddLocation(port, new(LocationNameText, LocationPathText));
            LocationNameText = string.Empty;
            LocationPathText = string.Empty;
        });

        RemoveLocationCmd = new CommonCommand((parm) =>
        {
            var name = (parm as string)!;
            _peControl.RemoveLocation(CurrentInstance.Port, name);
        });

        bool isUpdatingInstance = false;

        SelectedPortChangeCmd = new CommonCommand((parm) =>
        {
            if (isUpdatingInstance || parm is null)
            {
                return;
            }
            var currentPort = (int)parm!;
            CurrentInstance = _peControl.GetInstance(currentPort);
        });

        AddInstanceCmd = new CommonCommand((_) =>
        {
            int instancePort;
            var canParse = int.TryParse(InstancePortText, out instancePort);
            if (!canParse)
            {
                return;
            }
            if (instancePort < 0 || instancePort > 65535)
            {
                return;
            }
            if (!_peControl.IsPortUnique(instancePort))
            {
                return;
            }
            _peControl.AddInstance(instancePort);
            UpdatePorts();
            isUpdatingInstance = true;
            CurrentInstance = _peControl.GetInstance(instancePort);
            isUpdatingInstance = false;
            InstancePortText = string.Empty;
        });

        RemoveInstanceCmd = new CommonCommand((_) =>
        {
            var confirm = HcMessageBox.Show("Delete this instance?", "CONFIRM", MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }
            int currentPort = CurrentInstance.Port;
            _ = _peControl.StopInstanceAsync(currentPort);
            _peControl.RemoveInstance(currentPort);
            UpdatePorts();
            if (Ports.Count == 0)
            {
                _peControl.AddInstance(80);
            }
            UpdatePorts();
            isUpdatingInstance = true;
            CurrentInstance = _peControl.GetInstance(Ports.First());
            isUpdatingInstance = false;
        });

        SetInstanceEnableCmd = new CommonCommand((_) =>
        {
            if (CurrentInstance.Enable)
            {
                _ = _peControl.StopInstanceAsync(CurrentInstance.Port);
            }
            else
            {
                _ = _peControl.StartInstanceAsync(CurrentInstance.Port);
            }
        });

        SwitchAccessLock = new CommonCommand((_) =>
        {
            if (string.IsNullOrWhiteSpace(CurrentInstance.Password) && !CurrentInstance.IsLocked)
            {
                _peControl.SetInstancePassword(CurrentInstance, string.Empty);
                return;
            }

            _peControl.SetInstanceLocked(CurrentInstance, !CurrentInstance.IsLocked);
            _peControl.SetInstancePassword(CurrentInstance, CurrentInstance.Password);
        });

    }

}





public partial class PocketExplorerUI : Page
{
    public PocketExplorerViewModel peViewModel = new();

    public PocketExplorerUI()
    {
        InitializeComponent();
        DataContext = peViewModel;
    }

}

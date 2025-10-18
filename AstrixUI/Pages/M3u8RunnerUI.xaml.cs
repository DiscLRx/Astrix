using Microsoft.Win32;
using PocketExplorer.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AstrixUI.Pages;

public class M3u8RunnerViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public string RuntimeMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    private string _runtimeMessageBuffer;

    public bool M3u8LinkIsChecked
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool M3u8LocalFileIsChecked
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string M3u8Link
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string InputFile
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    public string OutputDirectory
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    public string UrlBase
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string VideoName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string? Referer
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    public int TasksCountLimit
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsRunButtonEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsRetryRunning
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int DoneCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int FailedCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsTempDirectoryFlyoutOpen
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand SelectInputFileCmd { get; set; }
    public ICommand SelectOutputDirectoryCmd { get; set; }
    public ICommand OpenOutputDirectoryCmd { get; set; }
    public ICommand Run { get; set; }
    public ICommand Retry { get; set; }
    public ICommand ToggleTempDirectoryFlyoutOpen { get; set; }
    public ICommand SetTempDirectoryFlyoutClosed { get; set; }
    public ICommand OpenTemp { get; set; }
    public ICommand ClearTemp { get; set; }

    private M3u8Runner.Executor? _executor;

    private readonly ScrollViewer _logScrollViewer;

    private StreamWriter? _logWriter;

    public M3u8RunnerViewModel(ScrollViewer logScrollViewer)
    {
        _logScrollViewer = logScrollViewer;
        RuntimeMessage = string.Empty;
        _runtimeMessageBuffer = string.Empty;
        M3u8Link = string.Empty;
        M3u8LinkIsChecked = true;
        InputFile = string.Empty;
        M3u8LocalFileIsChecked = false;
        OutputDirectory = "out";
        UrlBase = string.Empty;
        VideoName = "video";
        Referer = null;
        TasksCountLimit = 20;
        IsRunButtonEnabled = true;
        IsRetryRunning = false;
        DoneCount = 0;
        FailedCount = 0;
        Directory.CreateDirectory("data/m3u8runner/");

        SelectInputFileCmd = new CommonCommand((_) =>
        {
            var openFileDialog = new OpenFileDialog()
            {
                RootDirectory = "~",
                Multiselect = false
            };
            openFileDialog.ShowDialog();
            InputFile = openFileDialog.FileName;
        });

        SelectOutputDirectoryCmd = new CommonCommand((_) =>
        {
            var openFolderDialog = new OpenFolderDialog()
            {
                RootDirectory = "~",
                Multiselect = false
            };
            openFolderDialog.ShowDialog();
            OutputDirectory = openFolderDialog.FolderName;
        });

        OpenOutputDirectoryCmd = new CommonCommand(_ =>
        {
            var dir = System.IO.Path.GetFullPath(OutputDirectory);
            Process.Start("explorer.exe", dir);
        });

        Run = new CommonCommand(_ =>
        {
            _executor = new M3u8Runner.Executor();
            var t = Task.Run(() =>
            {
                IsRunButtonEnabled = false;
                var allDownloadFinished = false;
                if (M3u8LinkIsChecked)
                {
                    allDownloadFinished = _executor.DownloadByLink(M3u8Link, UrlBase, Referer, TasksCountLimit);
                }
                else
                {
                    allDownloadFinished = _executor.DownloadByLocalFile(InputFile, UrlBase, Referer, TasksCountLimit);
                }
                DoneCount = _executor.DownloadedCount;
                FailedCount = _executor.RetryInfos.Count;
                if (allDownloadFinished)
                {
                    var finalVideoName = _executor.ConvertToMp4(OutputDirectory, VideoName);
                    if(finalVideoName is not null)
                    {
                        VideoName = finalVideoName;
                    }
                }
            }).ContinueWith((_) =>
            {
                IsRunButtonEnabled = true;
                FlushLogBuffer();
            });
        });

        Retry = new CommonCommand(_ =>
        {
            if (_executor is null || FailedCount == 0)
            {
                return;
            }

            var t = Task.Run(() =>
            {
                IsRetryRunning = true;
                var allDownloadFinished = _executor.RetryDownload();
                DoneCount = _executor.DownloadedCount;
                FailedCount = _executor.RetryInfos.Count;
                if (allDownloadFinished)
                {
                    var finalVideoName = _executor.ConvertToMp4(OutputDirectory, VideoName);
                    if (finalVideoName is not null)
                    {
                        VideoName = finalVideoName;
                    }
                }
            }).ContinueWith((_) =>
            {
                IsRetryRunning = false;
                FlushLogBuffer();
            });
        });

        ToggleTempDirectoryFlyoutOpen = new CommonCommand(_ =>
        {
            IsTempDirectoryFlyoutOpen = !IsTempDirectoryFlyoutOpen;
        });

        SetTempDirectoryFlyoutClosed = new CommonCommand(_ =>
        {
            IsTempDirectoryFlyoutOpen = false;
        });

        OpenTemp = new CommonCommand(_ =>
        {
            var dir = System.IO.Path.GetFullPath(M3u8Runner.Values.VideoTempDirectory);
            if (Directory.Exists(dir))
            {
                Process.Start("explorer.exe", dir);
            }
            else
            {
                MessageHandler("临时目录不存在");
            }
        });

        ClearTemp = new CommonCommand(_ =>
        {
            if (Directory.Exists(M3u8Runner.Values.VideoTempDirectory))
            {
                Directory.Delete(M3u8Runner.Values.VideoTempDirectory, true);
                MessageHandler("临时目录已删除");
            }
            else
            {
                MessageHandler("临时目录不存在");
            }
        });

    }
    
    public void MessageHandler(string msg)
    {
        _logScrollViewer.Dispatcher.InvokeAsync(() =>
        {
            var dateText = $"{DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff] ")}";
            if (string.IsNullOrWhiteSpace(RuntimeMessage))
            {
                RuntimeMessage += msg;
                _runtimeMessageBuffer += dateText + msg;
            }
            else
            {
                RuntimeMessage += Environment.NewLine + msg;
                _runtimeMessageBuffer += Environment.NewLine + dateText + msg;
            }
            _logScrollViewer.ScrollToBottom();
            if (_runtimeMessageBuffer.Length > 2048)
            {
                FlushLogBuffer();
            }
        });
    }

    private readonly Lock _logLock = new();

    public void FlushLogBuffer() 
    {
        lock (_logLock)
        {
            _logWriter = new StreamWriter(File.Open("data/m3u8runner/runtime.log", FileMode.Append, FileAccess.Write));
            _logWriter!.Write(_runtimeMessageBuffer);
            _logWriter.Flush();
            _runtimeMessageBuffer = string.Empty;
            _logWriter.Dispose();
        }
    }


}


public partial class M3u8RunnerUI : Page
{
    public M3u8RunnerViewModel m3u8rViewModel;

    public M3u8RunnerUI()
    {
        InitializeComponent();
        m3u8rViewModel = new(logScrollViewer);
        DataContext = m3u8rViewModel;
    }

}

public class FailedCountGreaterThan0Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)value > 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MessageHandler
{

}
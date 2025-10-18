using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace M3u8Runner;

public class Executor
{
    private string? _outM3u8File = null;
    public List<DownloadInfo> RetryInfos = [];
    public int DownloadedCount = 0;
    private string? _urlBase;
    private string? _referer;
    private int _tasksCountLimit;
    private Action<string>? _messageCallBack;

    public Executor(Action<string>? messageCallback = null)
    {
        _messageCallBack = messageCallback;
        if (Directory.Exists(Values.VideoTempDirectory))
        {
            Directory.Delete(Values.VideoTempDirectory, true);
        }
        Directory.CreateDirectory(Values.VideoTempDirectory);
    }

    public bool DownloadByLocalFile(string inputFile, string urlBase, string? referer = null, int tasksCountLimit = 10)
    {
        _urlBase = urlBase;
        _referer = referer;
        _tasksCountLimit = tasksCountLimit;

        var fileHandler = new M3u8FileHandler(inputFile);
        _outM3u8File = fileHandler.OutM3u8File;
        List<DownloadInfo> downloadInfos = [];
        while (true)
        {
            var info = fileHandler.NextDownloadInfo();
            if (info is null)
            {
                break;
            }
            downloadInfos.Add(info);
        }
        var downloader = new Downloader(_urlBase, _referer, _tasksCountLimit);
        RetryInfos = downloader.StartDownload(downloadInfos, ref DownloadedCount, _messageCallBack);
        return RetryInfos.Count == 0;
    }

    public bool DownloadByLink(string m3u8Link, string urlBase, string? referer = null, int tasksCountLimit = 10)
    {
        var indexPath = Path.Join(Values.VideoTempDirectory, "index.m3u8");
        try
        {
            using var client = new HttpClient();
            if (referer is not null)
            {
                client.DefaultRequestHeaders.Referrer = new(referer);
            }
            using var m3u8Stream = client.GetStreamAsync(m3u8Link).Result;
            using var indexFileStream = File.OpenWrite(indexPath);
            m3u8Stream.CopyTo(indexFileStream);
        }
        catch(Exception e)
        {
            _messageCallBack?.Invoke($"无法从指定链接'{m3u8Link}'下载m3u8文件: {e.Message}");
            return false;
        }
        return DownloadByLocalFile(indexPath, urlBase, referer, tasksCountLimit);
    }

    public bool RetryDownload()
    {
        var downloader = new Downloader(_urlBase!, _referer, _tasksCountLimit);
        RetryInfos = downloader.StartDownload(RetryInfos, ref DownloadedCount, _messageCallBack);
        return RetryInfos.Count == 0;
    }

    public string? ConvertToMp4(string outputDirectory, string videoName)
    {
        if (_outM3u8File is null)
        {
            _messageCallBack?.Invoke("没有找到m3u8文件");
            return null;
        }
        if (!Directory.Exists(outputDirectory))
        {
            try
            {
                Directory.CreateDirectory(outputDirectory);
            }
            catch(Exception e)
            {
                _messageCallBack?.Invoke($"无法创建目录{outputDirectory}: {e.Message}");
            }
        }
        var outVideoTempPath = Path.Join(Values.VideoTempDirectory, $"{videoName}.mp4");

        var ffmpeg = Path.GetFullPath(Values.FFmpegPath);

        List<string> ffmpegArgs =
        [
            "-allowed_extensions", "ALL",
            "-i", _outM3u8File!,
            "-c:v", "copy",
            outVideoTempPath
        ];

        var ffmpegCommandLine = Values.FFmpegPath + " " + string.Join(' ', ffmpegArgs);
        _messageCallBack?.Invoke($"运行FFmpeg, 命令行: {ffmpegCommandLine}");

        var ffmpegStartInfo = new ProcessStartInfo()
        {
            FileName = ffmpeg,
        };
        ffmpegArgs.ForEach(ffmpegStartInfo.ArgumentList.Add);
        var process = new Process()
        {
            StartInfo = ffmpegStartInfo,
        };
        process.Start();
        process.WaitForExit();
        _messageCallBack?.Invoke($"FFmpeg已退出, 代码为{process.ExitCode}");

        var outVideoPath = Path.Join(Path.GetFullPath(outputDirectory), $"{videoName}.mp4");
        if (File.Exists(outVideoPath))
        {
            var outTempVideoStream = File.OpenRead(outVideoTempPath);
            var outVideoHash = MD5.Create().ComputeHash(outTempVideoStream);
            outTempVideoStream.Dispose();
            videoName = Convert.ToHexString(outVideoHash);
            outVideoPath = Path.Join(Path.GetFullPath(outputDirectory), $"{videoName}.mp4");
        }

        File.Move(outVideoTempPath, outVideoPath, true);
        _messageCallBack?.Invoke($"移动文件: {outVideoTempPath} -> {outVideoPath}");
        Directory.Delete(Values.VideoTempDirectory, true);
        return videoName;
    }
}

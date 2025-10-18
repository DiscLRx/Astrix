using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace M3u8Runner;

public record DownloadInfo(string RemotePath, string LocalFileName);

public class Downloader
{
    private string _urlBase;
    private string? _referer;
    private int _tasksCountLimit;

    private static List<Task> TasksToWait = new();

    public Downloader(string urlBase, string? referer, int tasksCountLimit)
    {
        _urlBase = urlBase.TrimEnd('/');
        _referer = referer;
        _tasksCountLimit = tasksCountLimit;
    }

    public List<DownloadInfo> StartDownload(List<DownloadInfo> downloadInfos, ref int downloadedCount, Action<string>? messageCallBack = null)
    {
        var client = new HttpClient();
        if (_referer is not null)
        {
            client.DefaultRequestHeaders.Referrer = new(_referer);
        }

        var failedInfos = new List<DownloadInfo>();
        foreach (var info in downloadInfos)
        {
            if (TasksToWait.Count == _tasksCountLimit)
            {
                int finishedTaskIndex = Task.WaitAny(TasksToWait.ToArray());
                TasksToWait.RemoveAt(finishedTaskIndex);
            }

            var t = Task.Run(async () =>
            {
                var separator = string.IsNullOrWhiteSpace(_urlBase) ? "" : "/";
                var url = $"{_urlBase}{separator}{info.RemotePath.TrimStart('/')}";
                var filePath = Path.Join(Path.GetFullPath(Values.VideoTempDirectory), info.LocalFileName);

                bool success = false;
                int retryTimesCount = 0;

                do
                {
                    try
                    {
                        var responseStream = await client.GetStreamAsync(url);
                        using var fileStream = File.OpenWrite(filePath);
                        await responseStream.CopyToAsync(fileStream);
                        success = true;
                        messageCallBack?.Invoke($"成功: {url} -> {filePath}");
                    }
                    catch(Exception e)
                    {
                        messageCallBack?.Invoke($"失败: {url} -> {filePath}; {e.Message}");
                        File.Delete(filePath);
                        retryTimesCount++;
                    }
                } while (!success && retryTimesCount < 5);

                if (!success)
                {
                    failedInfos.Add(info);
                }
            });

            TasksToWait.Add(t);
        }
        Task.WaitAll(TasksToWait.ToArray());
        downloadedCount += downloadInfos.Count - failedInfos.Count;
        return failedInfos;
    }

}


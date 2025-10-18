using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PocketExplorer.Web.Utils;

public class FileHelper
{
    public static string? GetFullPath(string root, string path)
    {
        path = path.Trim('/').Trim('\\');
        var fullPath = Path.Combine(root, path);
        return fullPath.StartsWith(root) ? fullPath : null;
    }

    public static bool FileExists(string root, string path)
    {
        var fullPath = GetFullPath(root, path);
        return fullPath is not null && File.Exists(fullPath);
    }

    public static (List<DirectoryItem>?, Dictionary<string, byte[]>?) GetDirItems(string root, string path)
    {
        var items = new ConcurrentBag<DirectoryItem>();

        var basePath = GetFullPath(root, path);
        if (basePath == null || !Directory.Exists(basePath))
        {
            return (null, null);
        }

        var taskDispatch = new TaskDispatch(20);
        ConcurrentDictionary<string, byte[]> thumbnailDict = [];

        var dirs = Directory.GetDirectories(basePath);
        foreach (var dir in dirs)
        {
            taskDispatch.AddTask(new Task(() =>
            {
                var dirInfo = new DirectoryInfo(dir);
                items.Add(new DirectoryItem($"{dirInfo.Name}/", 0, "dir", dirInfo.LastWriteTime));
            }));
        }

        var files = Directory.GetFiles(basePath);
        foreach (var file in files)
        {
            taskDispatch.AddTask(new Task(() =>
            {
                var fileInfo = new FileInfo(file);

                if (ThumbnailSupportChecker.Check(fileInfo.Extension))
                {
                    var bitmap = WindowsThumbnailProvider.GetThumbnail(file, 150, 150, ThumbnailOptions.ThumbnailOnly);
                    using var memoryStream = new MemoryStream();
                    bitmap.Save(memoryStream, ImageFormat.Jpeg);
                    var bytes = memoryStream.ToArray();
                    var thumbnailId = Random.Shared.NextInt64(100000000, long.MaxValue).ToString();
                    while (thumbnailDict.ContainsKey(thumbnailId))
                    {
                        thumbnailId = Random.Shared.NextInt64(100000000, long.MaxValue).ToString();
                    }

                    thumbnailDict[thumbnailId] = bytes;
                    items.Add(
                        new DirectoryItem(fileInfo.Name, fileInfo.Length, "file", fileInfo.LastWriteTime, thumbnailId));
                }
                else
                {
                    items.Add(new DirectoryItem(fileInfo.Name, fileInfo.Length, "file", fileInfo.LastWriteTime));
                }
            }));
        }

        taskDispatch.WaitAll();
        return (items.ToList(), thumbnailDict.ToDictionary());
    }
}

public class DirectoryItem(string name, long size, string type, DateTime lastModify, string? thumbnailId = null)
    : IComparable<DirectoryItem>
{
    public string Name { get; set; } = name;
    public long Size { get; set; } = size;
    public string Type { get; set; } = type;
    public DateTime LastModify { get; set; } = lastModify;
    public string? ThumbnailId { get; set; } = thumbnailId;

    [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string a, string b);

    public static bool operator >(DirectoryItem item1, DirectoryItem item2)
    {
        switch (item1.Type)
        {
            case "dir" when item2.Type == "file":
                return false;
            case "file" when item2.Type == "dir":
                return true;
            default:
            {
                var i = StrCmpLogicalW(item1.Name, item2.Name);
                return i > 0;
            }
        }
    }

    public static bool operator <(DirectoryItem item1, DirectoryItem item2)
    {
        switch (item1.Type)
        {
            case "dir" when item2.Type == "file":
                return true;
            case "file" when item2.Type == "dir":
                return false;
            default:
            {
                var i = StrCmpLogicalW(item1.Name, item2.Name);
                return i < 0;
            }
        }
    }

    public int CompareTo(DirectoryItem? other)
    {
        if (other == null)
        {
            return 1;
        }

        if (this > other)
        {
            return 1;
        }

        if (this < other)
        {
            return -1;
        }

        return 0;
    }
}
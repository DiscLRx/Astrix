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
        if (fullPath is null)
        {
            return false;
        }
        return File.Exists(fullPath);
    }

    public static List<DirectoryItem>? GetDirItems(string root, string path)
    {
        var items = new List<DirectoryItem>();

        var basePath = GetFullPath(root, path);
        if (basePath == null)
        {
            return null;
        }

        if (!Directory.Exists(basePath))
        {
            return null;
        }

        var dirs = Directory.GetDirectories(basePath);
        foreach (var dir in dirs)
        {
            var dirInfo = new DirectoryInfo(dir);
            items.Add(new($"{dirInfo.Name}/", 0, "dir", dirInfo.LastWriteTime));
        }
        var files = Directory.GetFiles(basePath);
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            items.Add(new(fileInfo.Name, fileInfo.Length, "file", fileInfo.LastWriteTime));
        }

        return items;
    }

}

public class DirectoryItem(string name, long size, string type, DateTime lastModify)
{
    public string Name { get; set; } = name;
    public long Size { get; set; } = size;
    public string Type { get; set; } = type;
    public DateTime LastModify { get; set; } = lastModify;
}

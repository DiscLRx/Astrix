using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace M3u8Runner;


internal class M3u8FileHandler
{
    private StreamReader _reader;
    private StreamWriter _writer;
    private int _currentFileSerial = 0;
    public string OutM3u8File = "index";

    public M3u8FileHandler(string inputFile)
    {
        _reader = new StreamReader(inputFile);
        int num = 0;
        while (File.Exists(Path.Join(Values.VideoTempDirectory, $"{OutM3u8File}.m3u8")))
        {
            OutM3u8File = $"index{++num}";
        }
        OutM3u8File = Path.Join(Values.VideoTempDirectory, $"{OutM3u8File}.m3u8");
        _writer = new StreamWriter(OutM3u8File);

    }

    public DownloadInfo? NextDownloadInfo()
    {
        while (true)
        {
            var lineText = _reader.ReadLine();
            if (lineText is null)
            {
                _reader.Dispose();
                _writer.Dispose();
                break;
            }

            if (lineText.StartsWith('#'))
            {
                if (lineText.StartsWith("#EXTINF"))
                {
                    _writer.WriteLine(lineText);
                    continue;
                }
                else if (lineText.StartsWith("#EXT-X-KEY"))
                {
                    var uriIndex = lineText.IndexOf("URI=");
                    if (uriIndex != -1)
                    {
                        var firstQuoteIndex = lineText.IndexOf('"', uriIndex);
                        var secondQuoteIndex = lineText.IndexOf('"', firstQuoteIndex + 1);
                        if (firstQuoteIndex != -1 && secondQuoteIndex != -1)
                        {
                            var keyUri = lineText[(firstQuoteIndex + 1)..secondQuoteIndex];
                            var localFileName = "video.key";
                            var newLineText = lineText[..(firstQuoteIndex + 1)] + localFileName + lineText[secondQuoteIndex..];
                            _writer.WriteLine(newLineText);
                            return new DownloadInfo(keyUri, localFileName);
                        }
                    }
                }
                else if (lineText.StartsWith("#EXT-X-MAP"))
                {
                    var uriIndex = lineText.IndexOf("URI=");
                    if (uriIndex != -1)
                    {
                        var firstQuoteIndex = lineText.IndexOf('"', uriIndex);
                        var secondQuoteIndex = lineText.IndexOf('"', firstQuoteIndex + 1);
                        if (firstQuoteIndex != -1 && secondQuoteIndex != -1)
                        {
                            var mapUri = lineText[(firstQuoteIndex + 1)..secondQuoteIndex];
                            var localFileName = $"map{GetExtension(mapUri)}";
                            var newLineText = lineText[..(firstQuoteIndex + 1)] + localFileName + lineText[secondQuoteIndex..];
                            _writer.WriteLine(newLineText);
                            return new DownloadInfo(mapUri, localFileName);
                        }
                    }
                }
                _writer.WriteLine(lineText); 
            }
            else
            {
                _currentFileSerial++;
                var localFileName = $"{_currentFileSerial}{GetExtension(lineText)}";
                _writer.WriteLine(localFileName);
                return new DownloadInfo(lineText, localFileName);
            }
        }
        return null;
    }

    private static string GetExtension(string url)
    {
        if (url.Contains('?'))
        {
            var pathPart = url.Split('?')[0];
            var index1 = pathPart.LastIndexOf('.');
            var index2 = url.IndexOf('?');
            return url[index1..index2];
        }
        else
        {
            var index = url.LastIndexOf('.');
            return url[index..];
        }
    }
}

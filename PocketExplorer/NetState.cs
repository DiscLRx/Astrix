using System.Net;
using System.Net.NetworkInformation;

namespace PocketExplorer;

public class NetState
{
    public static bool IsPortUsing(int port)
    {
        var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpListeners = ipProperties.GetActiveTcpListeners();
        var udpListeners = ipProperties.GetActiveUdpListeners();
        List<IPEndPoint> allListeners = [.. tcpListeners, .. udpListeners];
        return allListeners.Any(lsn => lsn.Port == port);
    }
}

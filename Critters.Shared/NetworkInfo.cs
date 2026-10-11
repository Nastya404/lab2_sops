using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Critters.Shared;

public static class NetworkInfo
{
    public static IEnumerable<string> GetLocalIPv4() =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                        n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(a => a.Address.ToString())
            .Distinct();

    public static string Format(System.Net.IPAddress? ip) =>
        ip is null ? "?" : (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).ToString();
}

using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PatitoServer.Lib;

namespace PatitoServer.Core;

public class Ip
{
    [JsonProperty("address")]
    public string Address { get; set; }
    
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public IpType Type { get; set; }

    public Ip(IpType type, string address)
    {
        Type = type;

        if (type == IpType.V4)
        {
            Address = address;
        }
        else
        {
            throw new NotImplementedException("IPv6 not implemented yet.");
        }
    }

    private static Ip Parse(IPEndPoint endPoint)
    {
        var address = endPoint.Address;

        var type = endPoint?
            .Address.AddressFamily == AddressFamily.InterNetworkV6
            ? IpType.V6
            : IpType.V4;

        return new Ip(type, address.ToString());
    }

    public static Ip GetRemoteIp(Socket? socket)
    {
        var remoteEndPoint = (IPEndPoint)socket?.RemoteEndPoint!;

        var ipRemote = Parse(remoteEndPoint);

        return ipRemote;
    }
    
    public static IPEndPoint GetLocalEndPoint(int port)
    {
        var localHost = Dns.GetHostEntry(Dns.GetHostName());
            
        Console.WriteLine("\nHostname: " + localHost.HostName + 
                          "\n\nAddressList:  \n" + 
                          string.Join("", localHost.AddressList.Select(ip => ip.ToString() + "\n").ToArray()));

        var localIpAddress = localHost.AddressList
                                 .Where(ip => ip.ToString() != Constants.IP_EXCLUDE)
                                 .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) 
                             ?? throw new System.Exception("No network adapters with an IPv4 address in the system!");

        var localEndPoint = new IPEndPoint(localIpAddress, port);

        Console.WriteLine("\nLocalEndPoint: " + localEndPoint.Address + "\n\n\n");

        return localEndPoint;
    }
}
using System.Net.Sockets;
using System.Net;

namespace PatitoServer.Core;

public class Client
{
    public Socket? Socket { get ; set ; }
    public string? Nickname { get ; set ; }
    public Ip ClientIp { get; set; }
    public bool IsOnline { get; set; }

    public Client(Socket? socket)
    {
        Socket = socket;
        ClientIp = GetRemoteIp();
        IsOnline = true;
    }

    private Ip GetRemoteIp()
    {
        var remoteEndPoint = (IPEndPoint)Socket?.RemoteEndPoint!;

        var ipRemote = Ip.Parse(remoteEndPoint);

        return ipRemote;
    }

    public void Disconect()
    {
        IsOnline = false;
        Socket?.Close();
    }
    public void Reconect()
    {
        IsOnline = true;
    }

    public override string ToString()
    {
        return $"{{\n \"Nickname\": \"{Nickname}\",\n  \"IpAddress\": \"{ClientIp.Address}\",\n  \"IpType\": \"{ClientIp.Type.ToString()}\",\n  \"IsOnline\": {IsOnline}\n}}";
    }
}
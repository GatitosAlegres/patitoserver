using System.Net.Sockets;
using Newtonsoft.Json;

namespace PatitoServer.Core;

public class Client
{
    [JsonIgnore]
    public Socket? Socket { get ; set ; }
    
    [JsonProperty("nickname")]
    public string? Nickname { get ; set ; }
    
    [JsonProperty("ip_address")]
    public Ip ClientIp { get; set; }
    
    [JsonProperty("is_online")]
    public bool IsOnline { get; set; }

    public Client(Socket? socket, Ip ip)
    {
        Socket = socket;
        ClientIp = ip;
        IsOnline = true;
    }
    
    [JsonConstructor]
    public Client(string nickname, Ip ip, bool isOnline)
    {
        Nickname = nickname;
        ClientIp = ip;
        IsOnline = isOnline;
    }

    public void Disconect()
    {
        IsOnline = false;
        Socket?.Disconnect(false);
    }
    public void Reconect()
    {
        IsOnline = true;
    }
}
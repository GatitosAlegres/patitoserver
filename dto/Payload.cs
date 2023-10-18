using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PatitoClient.Core;

namespace PatitoServer.Dto;

public record Payload
{
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public PayloadType Type { get; set; }
    
    [JsonProperty("data")]
    public byte[] Data { get; set; }

    public string RawData()
    {
        return Encoding.UTF8.GetString(Data);
    }
}
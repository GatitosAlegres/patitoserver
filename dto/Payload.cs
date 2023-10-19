using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PatitoClient.Core;

namespace PatitoServer.Dto;

public record Payload(PayloadType Type, byte[] Data)
{
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public PayloadType Type { get; set; } = Type;

    [JsonProperty("data")] public byte[] Data { get; set; } = Data;

    public string RawData()
    {
        return Encoding.UTF8.GetString(Data);
    }
}
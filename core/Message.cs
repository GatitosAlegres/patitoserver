using Newtonsoft.Json;

namespace PatitoServer.Core;

public class Message
{
    [JsonProperty("emitter")]
    private Client Emitter { get; set; }
    
    [JsonProperty("receiver")]
    private Client Receiver { get; set; }
    
    [JsonProperty("body")]
    private string Body { get; set; }

    public Message(string body, Client emitter, Client receiver)
    {
        Body = body;
        Emitter = emitter;
        Receiver = receiver;
    }
        
    public void Send(byte[] buffer)
    {
        Receiver.Socket?.Send(buffer);
    }
}
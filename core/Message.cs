using PatitoServer.Decoders;

namespace PatitoServer.Core;

public class Message
{

    private string Body { get; set; }
    private Client Emitter { get; set; }
    private Client Receiver { get; set; }

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
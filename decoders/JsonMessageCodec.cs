using PatitoServer.Core;

namespace PatitoServer.Decoders;

public class JsonMessageCodec : IMessageCodec
{
    public Message Decode(byte[] buffer, Func<string, Ip, Ip, Message?> result)
    {
        throw new NotImplementedException();
    }

    public byte[] Encode(Message message)
    {
        throw new NotImplementedException();
    }
}
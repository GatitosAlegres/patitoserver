using PatitoServer.Core;

namespace PatitoServer.Decoders;
public interface IMessageCodec
{
    Message Decode(byte[] buffer, Func<string, Ip, Ip, Message> result);

    byte[] Encode(Message message);
}
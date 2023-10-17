using PatitoServer.Core;
using PatitoServer.Internal;
using PatitoServer.Lib;
using PatitoServer.Decoders;

namespace PatitoServer;

// TODO:
// 1. Definir el formato de envio desde el cliente
// 2. Implementar los metodos de JSON Message Decoders
// 3. Asignar el nickname desde el cliente
public abstract class Entrypoint
{
    public static void Main(string[] args)
    {
        var jsonMessageCodec = new JsonMessageCodec();
        Client currentClient;

        var server = Server.Builder()
            .SetMaxClientsConnections(Constants.MAX_CLIENTS_CONNECTIONS)
            .SetMessageCodec(jsonMessageCodec)
            .Build();

        server.Listen(Constants.SERVER_PORT, () =>
        {
            currentClient = server.SyncAcceptClient();

            server.HandlerClient(currentClient);
        });
    }
}
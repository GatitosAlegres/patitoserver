using PatitoServer.Internal;
using PatitoServer.Lib;

namespace PatitoServer;

public abstract class Entrypoint
{
    public static void Main(string[] args)
    {

        var server = Server.Builder()
            .SetMaxClientsConnections(Constants.MAX_CLIENTS_CONNECTIONS)
            .Build();

        server.Listen(Constants.SERVER_PORT, () =>
        {
            Console.WriteLine("Server listen in port: " + Constants.SERVER_PORT + "\n\n");
        });
    }
}
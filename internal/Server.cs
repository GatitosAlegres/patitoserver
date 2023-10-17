using System.Net.Sockets;
using System.Text;
using PatitoServer.Core;
using PatitoServer.Decoders;
using PatitoServer.Lib;

namespace PatitoServer.Internal;

public class Server
{
    private int MaxClientsConnections { get; set; }
    private readonly Socket? _serverSocket;
    private List<Client> Clients { get; set; }
    private Thread? _handlerSessionThread;
    private Thread? _handlerPublishClientListThread;
    private int _count = 1;
    private IMessageCodec? MessageCodec { get; set; }
        
    private Server()
    {
        _serverSocket = new Socket(
            AddressFamily.InterNetwork, 
            SocketType.Stream, 
            ProtocolType.Tcp
        );
        Clients = new List<Client>();
    }

    public class ServerBuilder
    {
        private readonly Server _server = new();

        public ServerBuilder SetMaxClientsConnections(int maxClientsConnections)
        {
            _server.MaxClientsConnections = maxClientsConnections;
            return this;
        }

        public ServerBuilder SetMessageCodec(IMessageCodec messageCodec)
        {
            _server.MessageCodec = messageCodec;
            return this;
        }

        public Server Build()
        {
            return _server;
        }
    }
        
    public static ServerBuilder Builder()
    {
        return new ServerBuilder();
    }
        
    public void Listen(int port, Action callback)
    {

        var localEndPoint = Ip.GetLocalEndPoint(port);

        try
        {
            _serverSocket?.Bind(localEndPoint);

            _serverSocket?.Listen(MaxClientsConnections);

            Console.WriteLine("Server listen in port: " + Constants.SERVER_PORT + "\n\n");

            while (true)
            {
                callback.Invoke();
            }

        }
        catch (SocketException err)
        {
            Console.Error.WriteLine(err.Message);
        }
    }

    public Client SyncAcceptClient()
    {
        var clientSocket = _serverSocket?.Accept();

        Client client = new(clientSocket);

        client.Nickname = _count.ToString();

        _count++;
            
        Clients.Add(client);
            
        return client;
    }
        
    public void HandlerClient(Client client)
    {
        _handlerSessionThread = new Thread(CreateSession);
        _handlerPublishClientListThread = new Thread(PublishClientList);
            
        _handlerSessionThread?.Start(client);
        _handlerPublishClientListThread?.Start();
    }

    private void CreateSession(object? clt)
    {
        var client = (Client)clt!;

        var clientIp = client.ClientIp;

        Console.WriteLine($"Connection established with client: Ip Address {clientIp.Address} | Ip Type {clientIp.Type}");
            
        try
        {
            while (true)
            {
                var buffer = new byte[Constants.MAX_MESSAGE_SIZE];
                    
                var bytesRead = client.Socket!.Receive(buffer);

                if (bytesRead <= 0) continue;

                var message = MessageCodec?.Decode(buffer, (body, emitterIp, receiverIp) =>
                {
                    var emitter = Clients.Find(ctl => ctl.ClientIp.Address == emitterIp.Address);
                    var receiver = Clients.Find(ctl => ctl.ClientIp.Address == receiverIp.Address);

                    if (emitter == null || receiver == null) throw new Exception("emitter or receiver is null");
                    
                    var message = new Message(body, emitter, receiver);

                    return message;

                });

                if (message == null) continue;
                
                var messageBuffer = MessageCodec?.Encode(message)!;
                
                message.Send(messageBuffer);
            }
        }
        catch (Exception err)
        {
            Console.Error.WriteLine($"Connection error with client: Ip Address {clientIp.Address} | Ip Type {clientIp.Type}  \n {err.Message}");

        }
        finally
        {
            client.Socket?.Close();
        }
    }

    private void PublishClientList(object? obj)
    {
        var clientsString = string.Join(",", Clients);
            
        var responseData = Encoding.ASCII.GetBytes($"[{clientsString}]");
            
        Clients.ForEach(client =>
        {
            client.Socket?.Send(responseData);
        });
    }
}
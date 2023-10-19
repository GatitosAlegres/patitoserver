using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using PatitoClient.Core;
using PatitoServer.Core;
using PatitoServer.Dto;
using PatitoServer.Lib;

namespace PatitoServer.Internal;

public class Server
{
    private delegate void OnBroadcast();
    
    private int MaxClientsConnections { get; set; }
    private readonly Socket? _serverSocket;
    private List<Client> Clients { get; set; }
    private Thread? _handlerSessionThread;
    private OnBroadcast _publishClientListDelegate;

        
    private Server()
    {
        _serverSocket = new Socket(
            AddressFamily.InterNetwork, 
            SocketType.Stream, 
            ProtocolType.Tcp
        );
        Clients = new List<Client>();
        _publishClientListDelegate = PublishClientList;
    }

    public class ServerBuilder
    {
        private readonly Server _server = new();

        public ServerBuilder SetMaxClientsConnections(int maxClientsConnections)
        {
            _server.MaxClientsConnections = maxClientsConnections;
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

            callback.Invoke();
            
            while (true)
            {
                
                var clientSocket = _serverSocket?.Accept()!;

                var authenticatedClient =Auth(clientSocket);
                
                Clients.Add(authenticatedClient);
                
                _publishClientListDelegate.Invoke();
                
                HandlerClient(authenticatedClient);
            }
        }
        catch (SocketException err)
        {
            Console.Error.WriteLine(err.Message);
        }
    }

    private Client Auth(Socket clientSocketAccept)
    {
        var buffer = new byte[Constants.MAX_MESSAGE_SIZE];
                    
        var bytesRead = clientSocketAccept.Receive(buffer);
                
        var payloadRaw = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        
        var payload = DecodePayload(payloadRaw);
        
        if (payload == null) throw new Exception("Payload does not match");
        
        var clientJson = Encoding.UTF8.GetString(payload.Data);

        var clientAccept = JsonConvert.DeserializeObject<Client>(clientJson)!;

        clientAccept.ClientIp = Ip.GetRemoteIp(clientSocketAccept);
        clientAccept.Socket = clientSocketAccept;

        return clientAccept;
    }

    private void HandlerClient(Client? client)
    {
        _handlerSessionThread = new Thread(CreateSession);
        _handlerSessionThread?.Start(client);
    }
    
    private void CreateSession(object? clientObject)
    {
        
        var client = (Client)clientObject!;

        var clientIp = client.ClientIp;

        Console.WriteLine($"Connection established with client {client.Nickname}: Ip Address {clientIp.Address} | Ip Type {clientIp.Type}");
            
        try
        {
            while (true)
            {
                var buffer = new byte[Constants.MAX_MESSAGE_SIZE];
                    
                var bytesRead = client.Socket!.Receive(buffer);

                if (bytesRead <= 0) continue;
                
                var payloadRaw = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var payload = DecodePayload(payloadRaw);

                if (payload == null) continue;
                
                switch (payload.Type)
                {
                    case PayloadType.ACTION:
                    {
                        // TODO:
                        
                    }break;

                    case PayloadType.CLIENT_TO_CLIENT:
                    {
                        var rawData = payload.RawData();
                        
                        var message = JsonConvert.DeserializeObject<Message>(rawData)!;

                        var receiver = Clients.Find(clientAuth => clientAuth.Nickname == message.Receiver.Nickname &&
                                                                    clientAuth.ClientIp.Address == message.Receiver.ClientIp.Address);

                        var messageBytes = Encoding.UTF8.GetBytes(rawData);

                        var messagePayload = new Payload(PayloadType.CLIENT_TO_CLIENT, messageBytes);

                        var messagePayloadSerialized = JsonConvert.SerializeObject(messagePayload);

                        var messagePayloadBytes = Encoding.UTF8.GetBytes(messagePayloadSerialized);

                        receiver?.Socket?.Send(messagePayloadBytes);

                    }break;
                
                    default: Console.WriteLine("Error"); break;
                }
            }
        }
        catch (Exception err)
        {
            Console.Error.WriteLine($"Connection error with client: Ip Address {clientIp.Address} | Ip Type {clientIp.Type}  \n {err}");

        }
        finally
        {
            client.Socket?.Shutdown(SocketShutdown.Both);
            client.Socket?.Close();
            _publishClientListDelegate.Invoke();
        }
    }
    
    private Payload? DecodePayload(string raw)
    {
        var payload = JsonConvert.DeserializeObject<Payload>(raw);

        return payload;
    }

    private void PublishClientList()
    {
        var clientsString = JsonConvert.SerializeObject(Clients);

        var clientsBytes = Encoding.UTF8.GetBytes(clientsString);
        
        var byteString = "[" + string.Join(", ", clientsBytes) + "]";
            
        var responseData = Encoding.ASCII.GetBytes($"{{\"type\": \"BROADCAST\",\"data\": {byteString}}}");
        
        Clients.ForEach(client =>
        {
            if (client.Socket!.Connected)
            {
                client.Socket?.Send(responseData);
            }
        });
    }
}
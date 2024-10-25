using System.Net;
using System.Net.Sockets;
using Webserver.Settings;

namespace Webserver;

public class Server
{
    private readonly ServerSettings _settings;
    private TcpListener _listenerV4; // We will ONLY use tcp, we are reinventing the wheel here, so we don't need to support other protocols
    private TcpListener _listenerV6; // We will ONLY use tcp, we are reinventing the wheel here, so we don't need to support other protocols
    public Server(ServerSettings settings)
    {
        _settings = settings;
    }
    
    public void Start()
    {
        if (_settings.Host == "localhost")
        {
            _settings.Host = "127.0.0.1"; // fix ipaddress.parse bug
        }

        _listenerV4 = new TcpListener(System.Net.IPAddress.Parse(_settings.Host), _settings.Port);

        // Use IPv6Loopback if _settings.HostV6 is "::1"
        if (_settings.HostV6 == "::1")
        {
            _listenerV6 = new TcpListener(IPAddress.IPv6Loopback, _settings.Port);
        }
        else
        {
            _listenerV6 = new TcpListener(IPAddress.Parse(_settings.HostV6), _settings.Port);
        }
        
        ConfigureSocketOptions(_listenerV4.Server);
        ConfigureSocketOptions(_listenerV6.Server);

        _listenerV4.Start();
        _listenerV6.Start();
        
        Console.WriteLine($"Server started on {_settings.Host}:{_settings.Port} and {_settings.HostV6}:{_settings.Port}");
        
        // Start 2 threads, one for IPv4 and one for IPv6
        Task.Run(() => AcceptClients(_listenerV4));
        Task.Run(() => AcceptClients(_listenerV6));
    }
    
    private void AcceptClients(TcpListener listener)
    {
        while (true)
        {
            var client = listener.AcceptTcpClient();
            var clientHandler = new ClientHandler(client, _settings);
            Task.Run(() => clientHandler.Handle());
        }
    }
    
    private void ConfigureSocketOptions(Socket socket)
    {
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    }
}
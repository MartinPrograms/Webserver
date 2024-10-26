using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Webserver.Settings;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace Webserver;

public class ClientHandler
{
    private readonly TcpClient _client;
    private readonly ServerSettings _settings;
    private readonly X509Certificate2 _certificate;
    
    public ClientHandler(TcpClient client, ServerSettings settings)
    {
        _client = client;
        _settings = settings;

        if (settings.UseSSL)
        {
            _certificate = new X509Certificate2(settings.CertificatePath, settings.CertificatePassword);
        }
    }

    public void Handle()
    {
        if (_settings.UseSSL)
        {
            var sslStream = new SslStream(_client.GetStream(), false);
            sslStream.AuthenticateAsServer(_certificate, false, SslProtocols.Tls13, true);
            HandleRequest(sslStream);
        }
        else
        {
            HandleRequest(_client.GetStream());
        }
    }

    private void HandleRequest(Stream sslStream)
    {
        var reader = new StreamReader(sslStream);
        var writer = new StreamWriter(sslStream) { AutoFlush = true };

        var request = reader.ReadLine();

        var parts = request.Split(' ');
        var path = parts[1].Substring(1);

        if (path == "")
        {
            path = _settings.DefaultPage;
        }
        
        Console.WriteLine($"{parts[0]} {path} at {DateTime.Now}");
        
        if (path.EndsWith("/"))
        {
            path += _settings.DefaultPage;
        }
        
        if (!File.Exists(_settings.RootDirectory + path))
        {
            writer.WriteLine("HTTP/1.1 404 Not Found");
            writer.WriteLine($"Content-Type: text/html");
            
            var file = File.ReadAllText(_settings.Error404);
            writer.WriteLine($"Content-Length: {file.Length}");
            writer.WriteLine();
            writer.WriteLine(file);
            return;
        }
        
        var extension = Path.GetExtension(path);
        writer.WriteLine("HTTP/1.1 200 OK");
        writer.WriteLine($"Content-Type: {GetContentType(extension)}");
        
        var content = File.ReadAllText(_settings.RootDirectory + path);
        writer.WriteLine($"Content-Length: {content.Length}");
        writer.WriteLine();
        writer.WriteLine(content);
        
        _client.Close();
    }

    private string GetContentType(string extension)
    {
        return extension switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "text/javascript",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            _ => "text/plain"
        };
    }
}
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
            if (settings.CertificatePassword == null)
            {
                // Assume pem
                _certificate = CertificateManager.LoadCertificate(settings.CertificatePath, settings.CertificatePrivateKeyPath);
            }
            else
            {
                _certificate = new X509Certificate2(settings.CertificatePath, settings.CertificatePassword);
            }
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

    void HandleRequest(SslStream stream)
    {
        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream) { AutoFlush = true };

        ProcessRequest(reader, writer, stream);
        
        stream.Close();
        stream.Dispose();
    }
    
    void HandleRequest(NetworkStream stream)
    {
        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream) { AutoFlush = true };
        
        ProcessRequest(reader, writer, stream);
        
        stream.Close();
        stream.Dispose();
    }

    private void ProcessRequest(StreamReader reader, StreamWriter writer, Stream stream)
    {

        var request = reader.ReadLine();
        if (request == null)
        {
            return;
        }
        
        var parts = request.Split(' ');
        if (parts.Length != 3)
        {
            return;
        }
        
        var method = parts[0];
        var path = parts[1];
        var protocol = parts[2];
        
        if (method != "GET")
        {
            writer.WriteLine("HTTP/1.1 405 Method Not Allowed");
            return;
        }
        
        if (path == "/")
        {
            path = _settings.DefaultPage;
        }
        
        var filePath = Path.Combine(_settings.RootDirectory, path.TrimStart('/'));
        
        if (File.Exists(filePath))
        {
            var extension = Path.GetExtension(filePath);
            var contentType = GetContentType(extension);
            
            writer.WriteLine($"HTTP/1.1 200 OK");
            writer.WriteLine($"Content-Type: {contentType}");
            writer.WriteLine();
            
            using var fileStream = File.OpenRead(filePath);
            fileStream.CopyTo(stream);
        }
        else
        {
            writer.WriteLine($"HTTP/1.1 404 Not Found");
            writer.WriteLine($"Content-Type: text/html");
            writer.WriteLine();
            
            var errorPath = Path.Combine(_settings.RootDirectory, _settings.Error404);
            using var fileStream = File.OpenRead(errorPath);
            fileStream.CopyTo(stream);
        }
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
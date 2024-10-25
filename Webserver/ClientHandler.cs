using System.Net.Sockets;
using Webserver.Settings;

namespace Webserver;

public class ClientHandler
{
    private readonly TcpClient _client;
    private readonly ServerSettings _settings;

    public ClientHandler(TcpClient client, ServerSettings settings)
    {
        _client = client;
        _settings = settings;
    }

    public void Handle()
    {
        var stream = _client.GetStream();
        var reader = new StreamReader(stream);
        var writer = new StreamWriter(stream) { AutoFlush = true };

        var request = reader.ReadLine();
        if (request == null)
        {
            _client.Close();
            return;
        }

        var parts = request.Split(' ');
        var method = parts[0];
        var url = parts[1];
        var protocol = parts[2];

        Console.WriteLine($"{DateTime.Now.ToShortTimeString()} : {method} {url} {protocol}");

        var path = _settings.RootDirectory + url;
        if (url.EndsWith("/"))
        {
            path += _settings.DefaultPage;
        }

        if (File.Exists(path))
        {
            var extension = Path.GetExtension(path);
            var contentType = GetContentType(extension);
            var content = File.ReadAllText(path);

            writer.WriteLine($"HTTP/1.1 200 OK");
            writer.WriteLine($"Content-Type: {contentType}");
            writer.WriteLine($"Content-Length: {content.Length}");
            writer.WriteLine();
            writer.WriteLine(content);
        }
        else
        {
            // Read the 404 file
            if (File.Exists(_settings.Error404))
            {
                var content = File.ReadAllText(_settings.Error404);
                writer.WriteLine($"HTTP/1.1 404 Not Found");
                writer.WriteLine($"Content-Type: text/html");
                writer.WriteLine($"Content-Length: {content.Length}");
                writer.WriteLine();
                writer.WriteLine(content);
            }
            else
            {
                writer.WriteLine($"HTTP/1.1 404 Not Found");
                writer.WriteLine($"Content-Type: text/html");
                writer.WriteLine($"Content-Length: 0");
                writer.WriteLine();
            }
        }

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
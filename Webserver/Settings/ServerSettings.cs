namespace Webserver.Settings;

public class ServerSettings
{
    public string Host { get; set; } = "localhost"; // If set to 127.0.0.1, only local connections are allowed, if 0.0.0.0, all connections are allowed 
    public int Port { get; set; } = 80; // Standard HTTP port
    public string RootDirectory { get; set; } = "wwwroot"; 
    public bool DirectoryBrowsing { get; set; } = false; 
    
    public string ServerName { get; set; } = "Webserver"; // The name of the server, so we can listen for multiple servers on the same port
    
    public bool UseSSL { get; set; } = false; // If true, the server will use HTTPS instead of HTTP
    public string CertificatePath { get; set; } = "cert.pfx"; // The path to the certificate file
    public string CertificatePassword { get; set; } = "password"; // The password to the certificate file
    
    public string Error404 { get; set; } = "Default/404.html"; // The file to show if the file is not found
    
    public bool Redirects { get; set; } = true; // If true, the server will redirect to the correct URL if the URL contains a trailing slash
    public string RedirectUrl { get; set; } = "/"; // The URL to redirect to if Redirects is true, this could be another URL, for example "https://example.com", and it'd act as a proxy
    public bool ShowErrorDetails { get; set; } = true;
    public bool ShowStackTrace { get; set; } = true; 

    public string ExtensionPattern { get; set; } = "*.html"; // automatically add .html to the end of the URL if the file is not found
    public string DefaultPage { get; set; } = "index.html"; // The default page to look for if the URL is a directory
    public string HostV6 { get; set; } = "::1"; // Only local connections are allowed
}
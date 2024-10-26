using Webserver;
using Webserver.Settings;

var serverSettings = new ServerSettings();

serverSettings.Host = "0.0.0.0"; // Any
serverSettings.HostV6 = "::"; // Any
serverSettings.Port = 25501;
serverSettings.RootDirectory = @"Example/";

// SSL 
serverSettings.UseSSL = true;
serverSettings.CertificatePath = "test.pfx";
serverSettings.CertificatePassword = "testing";

var server = new Server(serverSettings);
server.Start();

Console.WriteLine("Press any key to stop the server");

Console.ReadLine();
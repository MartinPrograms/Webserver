using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Webserver;

public class CertificateManager
{
    public static X509Certificate2 LoadCertificate(string certPath, string keyPath)
    {
        if (Path.GetExtension(certPath) != ".pem" || Path.GetExtension(keyPath) != ".pem")
        {
            throw new ArgumentException("The certificate and key must be in PEM format");
        }
        
        var certParser = new PemReader(new StreamReader(certPath));
        var certificate = (Org.BouncyCastle.X509.X509Certificate)certParser.ReadObject();
        byte[] certEncoded = certificate.GetEncoded();

        var keyParser = new PemReader(new StreamReader(keyPath));
        var keyObject = keyParser.ReadObject() as AsymmetricCipherKeyPair;
        var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyObject.Private);

        var x509Cert = new X509Certificate2(certEncoded);
        x509Cert = x509Cert.CopyWithPrivateKey(RSA.Create(rsaParams));
        return x509Cert;
    }
}
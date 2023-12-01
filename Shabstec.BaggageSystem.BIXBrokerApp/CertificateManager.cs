using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;

namespace BIXBrokerApp_RabbitMQ
{
    public class CertificateManager
    {
        // Generate a self-signed X.509 certificate
        public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    new X500DistinguishedName($"CN={subjectName}"),
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // Set the certificate usage and add any desired extensions
                request.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));
                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                        false));

                // Create a self-signed certificate
                var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(10));

                // Return the certificate as an X509Certificate2 object
                return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
            }
        }

        // Load an X.509 certificate from a file
        public static X509Certificate2 LoadCertificateFromFile(string filePath, string password = null)
        {
            // Provide the file path and password if the certificate is password-protected
            return new X509Certificate2(filePath, password, X509KeyStorageFlags.Exportable);
        }
    }
}

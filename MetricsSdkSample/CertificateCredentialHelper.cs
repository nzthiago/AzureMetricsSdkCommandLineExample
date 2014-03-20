using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MetricsSdkSample
{
    internal static class CertificateCredentialHelper
    {
        internal static CertificateCloudCredentials CreateCredential(string subscriptionId, string certFilePath, string password)
        {
            return new CertificateCloudCredentials(subscriptionId, CreateFromFile(certFilePath, password));
        }

        internal static X509Certificate2 CreateFromFile(string fileName, string password)
        {
            return new X509Certificate2(fileName, password);
        }
    }
}

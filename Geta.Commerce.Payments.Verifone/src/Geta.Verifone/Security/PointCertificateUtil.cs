using System;
using System.Configuration;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Hosting;

namespace Geta.Verifone.Security
{
    /// <summary>
    /// Summary description for PointCertificateUtil
    /// </summary>
    public class PointCertificateUtil
    {
        public static X509Certificate2 GetMerchantCertificate()
        {
            return GetCertificate("Verifone:MerchantCertificatePath", "password");
        }

        public static X509Certificate2 GetPointCertificate()
        {
            return GetCertificate("Verifone:PointCertificatePath", "password");
        }

        private static X509Certificate2 GetCertificate(string certName, string password)
        {
            string certFilePath = ConfigurationManager.AppSettings[certName];

            if (string.IsNullOrWhiteSpace(certFilePath))
            {
                throw new ConfigurationErrorsException(string.Format("{0} is missing.", certName));
            }

            return new X509Certificate2(certFilePath, password, X509KeyStorageFlags.Exportable);
        }
    }
}
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
            return GetCertificate("Verifone:MerchantCertificatePath", "Verifone:MerchantCertificatePassword");
        }

        public static X509Certificate2 GetPointCertificate()
        {
            return GetCertificate("Verifone:PointCertificatePath", "Verifone:PointCertificatePassword");
        }

        private static X509Certificate2 GetCertificate(string certName, string passwordSettingKey)
        {
            string certFilePath = ConfigurationManager.AppSettings[certName];

            if (string.IsNullOrWhiteSpace(certFilePath))
            {
                throw new ConfigurationErrorsException(string.Format("{0} is missing.", certName));
            }

            string password = ConfigurationManager.AppSettings[passwordSettingKey];

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ConfigurationErrorsException(string.Format("{0} is missing.", passwordSettingKey));
            }

            return new X509Certificate2(certFilePath, password, X509KeyStorageFlags.Exportable);
        }
    }
}
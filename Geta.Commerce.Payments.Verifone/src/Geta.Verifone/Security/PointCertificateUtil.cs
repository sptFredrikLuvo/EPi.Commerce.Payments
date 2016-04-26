using System;
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
            //return GetCertificate("C:\\Projects\\EPi.Commerce.Payments\\Geta.Commerce.Payments.Verifone\\docs\\Keys\\demo-merchant-no.p12", "password");
            return GetCertificate("C:\\Projects\\EPi.Commerce.Payments\\Geta.Commerce.Payments.Verifone\\docs\\Keys\\demo-merchant-agreement.p12", "password");
        }

        public static X509Certificate2 GetPointCertificate()
        {
            return GetCertificate("C:\\Projects\\EPi.Commerce.Payments\\Geta.Commerce.Payments.Verifone\\docs\\Keys\\point-e-commerce-test-public-key.crt", "password");
        }

        private static X509Certificate2 GetCertificate(string certFilePath, string password)
        {
            return new X509Certificate2(certFilePath, password, X509KeyStorageFlags.Exportable);
        }
    }
}
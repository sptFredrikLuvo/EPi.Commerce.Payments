using System.Globalization;
using EPiServer.ServiceLocation;

namespace Geta.Commerce.Payments.Verifone.HostedPages
{
    [ServiceConfiguration(typeof(IVerifonePaymentService))]
    public class DefaultVerifonePaymentService : IVerifonePaymentService
    {
        public virtual string GetPaymentLocale(CultureInfo culture)
        {
            switch (culture.Name)
            {
                case "no":
                case "nb":
                case "nn":
                case "nb-NO":
                case "nn-NO":
                    return "no_NO";
                case "sv":
                case "sv-SE":
                    return "sv_SE";
                case "sv-FI":
                    return "sv_FI";
                case "fi-FI":
                case "fi":
                    return "fi_FI";
                case "da-DK":
                case "da":
                    return "dk_DK";
                default:
                    return "en_GB";
            }
        }
    }
}
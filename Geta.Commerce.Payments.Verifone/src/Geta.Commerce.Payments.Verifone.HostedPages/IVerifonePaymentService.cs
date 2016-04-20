using System.Globalization;

namespace Geta.Commerce.Payments.Verifone.HostedPages
{
    public interface IVerifonePaymentService
    {
        string GetPaymentLocale(CultureInfo culture);
    }
}
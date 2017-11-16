using System;
using System.Linq;
using Geta.Klarna.Checkout.Models;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    public class ProviderSettings
    {
        public ProviderSettings(bool isProduction, string merchant, string secret, string localeCode)
        {
            IsProduction = isProduction;
            CurrentLocale = Locale.Locales.FirstOrDefault(l => l.LocaleCode == localeCode);
            MerchantId = merchant;
            Secret = secret;
            OrderBaseUri = isProduction ? new Uri(KlarnaConstants.ProductionBaseUri) : new Uri(KlarnaConstants.TestBaseUri);
        }

        public bool IsProduction { get; private set; }
        public Locale CurrentLocale { get; private set; }
        public string MerchantId { get; private set; }
        public string Secret { get; private set; }
        public Uri OrderBaseUri { get; private set; }
    }
}

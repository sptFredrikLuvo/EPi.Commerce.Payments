using System;
using System.Linq;
using Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions;
using Geta.Klarna.Checkout.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    public static class PaymentHelper
    {
        public static ProviderSettings GetProviderSettings(ICurrentMarket currentMarket, string currentTwoLetterIsoLanguageName)
        {
            var paymentmethod = GetKlarnaPaymentMethod(currentMarket, currentTwoLetterIsoLanguageName);

            return GetProviderSettings(paymentmethod);
        }

        public static ProviderSettings GetProviderSettings(Guid paymentMethodId)
        {
            return GetProviderSettings(PaymentManager.GetPaymentMethod(paymentMethodId));
        }

        public static PaymentMethodDto GetKlarnaPaymentMethod(ICurrentMarket currentMarket, string currentTwoLetterIsoLanguageName)
        {
            var klarnaGatewayClass = typeof(KlarnaCheckoutPaymentGateway).FullName + ", " + typeof(KlarnaCheckoutPaymentGateway).Assembly.GetName().Name;
            var marketId = currentMarket.GetCurrentMarket().MarketId;
            var methods = PaymentManager.GetPaymentMethodsByMarket(marketId).PaymentMethod.Where(c => c.IsActive);

            var klarnaRow = methods.
                Where(paymentRow => currentTwoLetterIsoLanguageName.Equals(paymentRow.LanguageId, StringComparison.OrdinalIgnoreCase)).
                Where(paymentRow => paymentRow.ClassName == klarnaGatewayClass).
                OrderBy(paymentRow => paymentRow.Ordering).ToList().FirstOrDefault();

            if (klarnaRow == null)
            {
                string error = string.Format("Missing provider settings for current language: {0} and market: {1}. Please add missing configuration to Commerce Manager.", currentTwoLetterIsoLanguageName, marketId);
                throw new Exception(error);
            }

            return PaymentManager.GetPaymentMethod(klarnaRow.PaymentMethodId);
        }

        private static ProviderSettings GetProviderSettings(PaymentMethodDto klarnaPaymentMethodDto)
        {
            return new ProviderSettings(
                    bool.Parse(klarnaPaymentMethodDto.GetParameter(KlarnaConstants.IsProduction)),
                    klarnaPaymentMethodDto.GetParameter(KlarnaConstants.MerchantId),
                    klarnaPaymentMethodDto.GetParameter(KlarnaConstants.Secret),
                    klarnaPaymentMethodDto.GetParameter(KlarnaConstants.Locale)
                );
        }

    }
}

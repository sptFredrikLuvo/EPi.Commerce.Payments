using System.Collections.Generic;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class OrderDataExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this OrderData orderData)
        {
            return new Dictionary<string, object>
                    {
                        { "purchase_country", orderData.Locale.PurchaseCountry },
                        { "purchase_currency", orderData.Locale.PurchaseCurrency },
                        { "locale", orderData.Locale.LocaleCode },
                        { "merchant", orderData.Merchant.ToDictionary() },
                        { "cart", orderData.Cart.ToDictionary() },
                        { "options", orderData.Options.ToDictionary() }
                    };
        }
    }
}
using System.Collections.Generic;
using System.Data.Odbc;
using Geta.Klarna.Checkout.Models;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class OrderDataExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this OrderData orderData)
        {
            var jsonOrderData = new Dictionary<string, object>
                    {
                        { "purchase_country", orderData.Locale.PurchaseCountry },
                        { "purchase_currency", orderData.Locale.PurchaseCurrency },
                        { "locale", orderData.Locale.LocaleCode },
                        { "merchant", orderData.Merchant.ToDictionary() },
                        { "cart", orderData.Cart.ToDictionary() },
                        { "options", orderData.Options.ToDictionary() }
                    };

            if(orderData.ShippingAddress != null)
                jsonOrderData.Add("shipping_address", orderData.ShippingAddress.ToDictionary());

            return jsonOrderData;
        }
    }
}
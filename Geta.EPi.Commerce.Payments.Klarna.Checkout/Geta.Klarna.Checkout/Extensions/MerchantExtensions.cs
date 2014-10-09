using System;
using System.Collections.Generic;
using System.Web;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class MerchantExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this Merchant merchant)
        {
            return new Dictionary<string, object>
            {
                { "id", merchant.MerchantId },
                { "terms_uri", merchant.TermsUri },
                { "checkout_uri", merchant.CheckoutUri },
                { "confirmation_uri", AddKlarnaOrderToQueryString(merchant.ConfirmationUri) },
                { "push_uri", AddKlarnaOrderToQueryString(merchant.PushUri) }
            };
        }

        private static Uri AddKlarnaOrderToQueryString(Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["klarnaOrder"] = "{checkout.order.uri}";
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Web;
using Geta.Klarna.Checkout.Models;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class MerchantExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this Merchant merchant)
        {
            var merchantData = new Dictionary<string, object>
            {
                { "id", merchant.MerchantId },
                { "terms_uri", merchant.TermsUri },
                { "checkout_uri", merchant.CheckoutUri },
                { "confirmation_uri", AddKlarnaOrderToQueryString(merchant.ConfirmationUri) },
                { "push_uri", AddKlarnaOrderToQueryString(merchant.PushUri) }
            };

            if(merchant.ValidationUri != null)
                merchantData.Add("validation_uri", merchant.ValidationUri);

            return merchantData;
        }

        private static string AddKlarnaOrderToQueryString(Uri uri)
        {
            //NOTE: NOT using AddParameter method below because of issues with encodig of klarnaOrder parameter value contaning '{}' characters
            var currentUrlString = uri.AbsoluteUri;
            var queryParamAndValue = "klarnaOrder={checkout.order.id}";
            if (currentUrlString.Contains("?"))
                return $"{currentUrlString}&{queryParamAndValue}";
            return $"{currentUrlString}?{queryParamAndValue}";
        }

        /// <summary>
        /// Adds the specified parameter to the Query String.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramName">Name of the parameter to add.</param>
        /// <param name="paramValue">Value for the parameter to add.</param>
        /// <returns>Url with added parameter.</returns>
        public static Uri AddParameter(Uri url, string paramName, string paramValue)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[paramName] = paramValue;
            uriBuilder.Query = query.ToString();

            return new Uri(uriBuilder.ToString());
        }
    }
}
using System.Collections.Generic;
using Geta.Klarna.Checkout.Models;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class MerchantReferenceExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this MerchantReference merchantReference)
        {
            var result = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(merchantReference.OrderId1))
            {
                return result;
            }
            result.Add("orderid1", merchantReference.OrderId1);

            if (string.IsNullOrEmpty(merchantReference.OrderId2))
            {
                return result;
            }
            result.Add("orderid2", merchantReference.OrderId2);

            return result;
        }
    }
}
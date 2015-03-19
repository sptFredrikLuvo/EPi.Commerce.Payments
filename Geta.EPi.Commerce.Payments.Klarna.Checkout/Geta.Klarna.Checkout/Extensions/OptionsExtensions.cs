using System.Collections.Generic;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class OptionsExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this Options options)
        {
            return new Dictionary<string, object>
            {
                {"allow_seperate_shipping_address", options.AllowSeparateShippingAddress}
            };
        } 
    }
}
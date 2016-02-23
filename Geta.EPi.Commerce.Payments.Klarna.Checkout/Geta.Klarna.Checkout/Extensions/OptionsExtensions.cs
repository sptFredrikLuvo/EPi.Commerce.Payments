using System.Collections.Generic;
using Geta.Klarna.Checkout.Models;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class OptionsExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this Options options)
        {
            var optionsData = new Dictionary<string, object>
            {
                {"allow_separate_shipping_address", options.AllowSeparateShippingAddress}
            };

            if (!string.IsNullOrEmpty(options.ButtonColorCode))
            {
                optionsData.Add("color_button", options.ButtonColorCode);
                optionsData.Add("color_checkbox", options.ButtonColorCode);
            }
            return optionsData;
        }
    }
}
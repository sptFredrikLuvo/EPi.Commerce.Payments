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

            if (options.ColorOptions == null)
            {
                return optionsData;
            }

            if (!string.IsNullOrEmpty(options.ColorOptions.ButtonColorCode))
            {
                optionsData.Add("color_button", options.ColorOptions.ButtonColorCode);
            }

            if (!string.IsNullOrEmpty(options.ColorOptions.ButtonTextColorCode))
            {
                optionsData.Add("color_button_text", options.ColorOptions.ButtonTextColorCode);
            }

            if (!string.IsNullOrEmpty(options.ColorOptions.CheckboxColorCode))
            {
                optionsData.Add("color_checkbox", options.ColorOptions.CheckboxColorCode);
            }

            if (!string.IsNullOrEmpty(options.ColorOptions.CheckboxCheckmarkColorCode))
            {
                optionsData.Add("color_checkbox_checkmark", options.ColorOptions.CheckboxCheckmarkColorCode);
            }

            if (!string.IsNullOrEmpty(options.ColorOptions.HeaderColorCode))
            {
                optionsData.Add("color_header", options.ColorOptions.HeaderColorCode);
            }

            if (!string.IsNullOrEmpty(options.ColorOptions.LinkColorCode))
            {
                optionsData.Add("color_link", options.ColorOptions.LinkColorCode);
            }

            return optionsData;
        }
    }
}
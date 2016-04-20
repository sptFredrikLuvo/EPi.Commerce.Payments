using System;
using System.Globalization;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Extensions
{
    public static class DecimalExtensions
    {
        public static string ToAmountString(this decimal number)
        {
            return Math.Round(number, 2).ToString(CultureInfo.InvariantCulture).Replace(".", "");
            return ((int) Math.Round(number)).ToString(CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}
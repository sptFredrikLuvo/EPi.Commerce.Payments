using System;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment step helper class
    /// </summary>
    public static class PaymentStepHelper
    {
        /// <summary>
        /// Get amount as string value (without decimals)
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static string GetAmount(decimal amount)
        {
            return Math.Round(amount * 100).ToString();
        }
    }
}

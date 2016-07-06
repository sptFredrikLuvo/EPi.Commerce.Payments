using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Capture payment step
    /// </summary>
    public class CapturePaymentStep : PaymentStep
    {
        /// <summary>
        /// Process 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(Payment payment, ref string message)
        {
            if (payment.TransactionType == "Capture")
            {
                var orderForm = payment.Parent;
                var paymentMethoDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);

                var merchantId = paymentMethoDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
                var token = paymentMethoDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);

                var amount = PaymentStepHelper.GetAmount(orderForm.Total);
                this.Client.Capture(merchantId, token, payment.ProviderTransactionID, amount);

                return true;
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, ref message);
            }
            return false;
        }
    }
}

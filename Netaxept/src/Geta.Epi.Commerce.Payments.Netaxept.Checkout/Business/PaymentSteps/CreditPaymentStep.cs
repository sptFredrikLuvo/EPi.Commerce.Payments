using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Credit payment step
    /// </summary>
    public class CreditPaymentStep : PaymentStep
    {
        /// <summary>
        /// Process 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(Payment payment, ref string message)
        {
            if (payment.TransactionType == "Credit")
            {
                var orderForm = payment.Parent;
                var paymentMethoDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);

                var merchantId = paymentMethoDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
                var token = paymentMethoDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);

                var amount = PaymentStepHelper.GetAmount(payment.Amount);
                this.Client.Credit(merchantId, token, payment.ProviderTransactionID, amount);

                AddNote(orderForm, "Payment - Credit", "Payment - Amount is Credited");

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

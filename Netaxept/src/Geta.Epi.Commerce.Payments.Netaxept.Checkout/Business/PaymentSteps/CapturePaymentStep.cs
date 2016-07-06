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
        public CapturePaymentStep(Payment payment) : base(payment)
        { }

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
                
                var amount = PaymentStepHelper.GetAmount(orderForm.Total);
                this.Client.Capture(payment.ProviderTransactionID, amount);

                AddNote(orderForm, "Payment - Captured", "Payment - Amount is captured");

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

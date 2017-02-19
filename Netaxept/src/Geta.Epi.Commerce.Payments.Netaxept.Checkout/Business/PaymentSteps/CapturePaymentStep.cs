using System;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Capture payment step
    /// </summary>
    public class CapturePaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CapturePaymentStep));

        public CapturePaymentStep(IPayment payment) : base(payment)
        { }

        /// <summary>
        /// Process 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="orderGroup"></param>
        /// <param name="message"></param>
        /// <param name="orderForm"></param>
        /// <returns></returns>
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            if (payment.TransactionType == "Capture")
            {
                var amount = PaymentStepHelper.GetAmount(payment.Amount);

                try
                {
                    var result = this.Client.Capture(payment.ProviderTransactionID, amount);
                    if (result.ErrorOccurred)
                    {
                        message = result.ErrorMessage;
                        payment.Status = "Failed";
                        AddNoteAndSaveChanges(orderGroup, "Payment Captured - Error", "Payment - Captured - Error: " + result.ErrorMessage);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    message = ex.Message;
                    payment.Status = "Failed";
                    AddNoteAndSaveChanges(orderGroup, "Payment Captured - Error", "Payment - Captured - Error: " + ex.Message);
                    return false;
                }

                AddNoteAndSaveChanges(orderGroup, "Payment - Captured", "Payment - Captured");

                return true;
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, ref message);
            }
            return false;
        }
        
    }
}

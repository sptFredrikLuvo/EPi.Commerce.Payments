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
        public override PaymentStepResult Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup)
        {
            if (payment.TransactionType == "Capture")
            {
                var amount = PaymentStepHelper.GetAmount(payment.Amount);

                try
                {
                    var result = this.Client.Capture(payment.TransactionID, amount);
                    if (result.ErrorOccurred)
                    {
                        payment.Status = "Failed";
                        AddNoteAndSaveChanges(orderGroup, "Payment Captured - Error", "Payment - Captured - Error: " + result.ErrorMessage);
                        return Fail(result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    payment.Status = "Failed";
                    AddNoteAndSaveChanges(orderGroup, "Payment Captured - Error", "Payment - Captured - Error: " + ex.Message);
                    return Fail(ex.Message);
                }

                AddNoteAndSaveChanges(orderGroup, "Payment - Captured", "Payment - Captured");

                return Success();
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup);
            }

            return Fail(null);
        }
        
    }
}

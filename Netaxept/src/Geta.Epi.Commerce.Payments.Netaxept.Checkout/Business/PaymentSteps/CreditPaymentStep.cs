using System;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Credit payment step
    /// </summary>
    public class CreditPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(CreditPaymentStep));

        public CreditPaymentStep(IPayment payment) : base(payment)
        { }

        /// <summary>
        /// Process 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            if (payment.TransactionType == "Credit")
            {
                var amount = PaymentStepHelper.GetAmount(payment.Amount);

                try
                {
                    var result = this.Client.Credit(payment.TransactionID, amount);
                    if (result.ErrorOccurred)
                    {
                        message = result.ErrorMessage;
                        payment.Status = "Failed";
                        AddNoteAndSaveChanges(orderGroup, "Payment Credit - Error", "Payment Credit - Error: " + result.ErrorMessage);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    message = ex.Message;
                    payment.Status = "Failed";
                    AddNoteAndSaveChanges(orderGroup, "Payment Credit - Error", "Payment Credit - Error: " + ex.Message);
                    return false;
                }

                AddNoteAndSaveChanges(orderGroup, "Payment - Credited", "Payment - Credited");

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

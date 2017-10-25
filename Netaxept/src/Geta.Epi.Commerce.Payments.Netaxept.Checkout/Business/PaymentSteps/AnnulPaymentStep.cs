using System;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Credit payment step
    /// </summary>
    public class AnnulPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AnnulPaymentStep));

        public AnnulPaymentStep(IPayment payment) : base(payment)
        { }

        /// <summary>
        /// Process 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            if (payment.TransactionType == "Void")
            {
                try
                {
                    var result = this.Client.Annul(payment.TransactionID);
                    if (result.ErrorOccurred)
                    {
                        message = result.ErrorMessage;
                        payment.Status = "Failed";
                        AddNoteAndSaveChanges(orderGroup, "Payment Annul - Error", "Payment Annul - Error: " + result.ErrorMessage);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    message = ex.Message;
                    payment.Status = "Failed";
                    AddNoteAndSaveChanges(orderGroup, "Payment Annul - Error", "Payment Annul - Error: " + ex.Message);
                    return false;
                }

                AddNoteAndSaveChanges(orderGroup, "Payment - Annul", "Payment - Annulled");

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

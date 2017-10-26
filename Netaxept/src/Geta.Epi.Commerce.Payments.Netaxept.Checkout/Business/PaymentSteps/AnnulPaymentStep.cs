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
        public override PaymentStepResult Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup)
        {
            if (payment.TransactionType == "Void")
            {
                try
                {
                    var result = this.Client.Annul(payment.TransactionID);
                    if (result.ErrorOccurred)
                    {
                        payment.Status = "Failed";
                        AddNoteAndSaveChanges(orderGroup, "Payment Annul - Error", "Payment Annul - Error: " + result.ErrorMessage);
                        return Fail(result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    payment.Status = "Failed";
                    AddNoteAndSaveChanges(orderGroup, "Payment Annul - Error", "Payment Annul - Error: " + ex.Message);
                    return Fail(ex.Message);
                }

                AddNoteAndSaveChanges(orderGroup, "Payment - Annul", "Payment - Annulled");

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

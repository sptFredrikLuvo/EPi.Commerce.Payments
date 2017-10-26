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
        public override PaymentStepResult Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup)
        {
            if (payment.TransactionType == "Credit")
            {
                var amount = PaymentStepHelper.GetAmount(payment.Amount);

                try
                {
                    var result = this.Client.Credit(payment.TransactionID, amount);
                    if (result.ErrorOccurred)
                    {
                        payment.Status = "Failed";
                        AddNoteAndSaveChanges(orderGroup, "Payment Credit - Error", "Payment Credit - Error: " + result.ErrorMessage);
                        return Fail(result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    payment.Status = "Failed";
                    AddNoteAndSaveChanges(orderGroup, "Payment Credit - Error", "Payment Credit - Error: " + ex.Message);
                    return Fail(ex.Message);
                }

                AddNoteAndSaveChanges(orderGroup, "Payment - Credited", "Payment - Credited");

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

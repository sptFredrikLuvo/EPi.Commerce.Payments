using System;
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

        public CreditPaymentStep(Payment payment) : base(payment)
        { }

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

                var amount = PaymentStepHelper.GetAmount(payment.Amount);

                try
                {
                    var result = this.Client.Credit(payment.ProviderTransactionID, amount);
                    if (result.ErrorOccurred)
                    {
                        message = result.ErrorMessage;
                        AddNote(orderForm, "Payment Credit - Failed", "Error: " + result.ErrorMessage);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    message = ex.Message;
                    AddNote(orderForm, "Payment Credit - Failed", "Error: " + ex.Message);
                    return false;
                }

                AddNote(orderForm, "Payment - Credit", "Payment - Amount is credited");

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

using System;
using EPiServer.Logging;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Models;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment should authenticated at Netaxept
    /// </summary>
    public class AuthorizationPaymentStep : PaymentStep
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(AuthorizationPaymentStep));

        public AuthorizationPaymentStep(Payment payment) : base(payment)
        { }

        /// <summary>
        /// This step is not used in the chain.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(Payment payment, ref string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Process payment
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public PaymentAuthorizationResult Process(Payment payment, string transactionId)
        {
            var result = new PaymentAuthorizationResult();
            if (payment.TransactionType == "Authorization" && !string.IsNullOrEmpty(transactionId))
            {
                var orderForm = payment.Parent;

                PaymentResult paymentResult = null;
                try
                {
                    paymentResult = this.Client.Query(transactionId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);

                    AddNote(payment.Parent, "Payment Query - Error", "Payment Query - Error: " + ex.Message, true);
                    return result;
                }
                
                if (paymentResult.Cancelled)
                {
                    payment.Status = "Failed";
                    result.Result = PaymentResponseCode.Cancelled;
                    return result;
                }
                if (paymentResult.ErrorOccurred)
                {
                    payment.Status = "Failed";
                    result.Result = PaymentResponseCode.ErrorOccurred;
                    result.ErrorMessage = paymentResult.ErrorMessage;

                    AddNote(payment.Parent, "Payment - Failed", "Payment - Error: " + result.ErrorMessage, true);
                    return result;
                }

                try
                {
                    var responseResult = this.Client.Authorize(transactionId);
                    if (responseResult.ErrorOccurred)
                    {
                        AddNote(orderForm, "Payment Auth - Error", "Payment Auth - Error: " + responseResult.ErrorMessage, true);

                        payment.Status = "Failed";
                        result.Result = PaymentResponseCode.ErrorOccurred;

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    payment.Status = "Failed";
                    result.Result = PaymentResponseCode.ErrorOccurred;
                    result.ErrorMessage = ex.Message;

                    Logger.Error(ex.Message);

                    AddNote(payment.Parent, "Payment Auth - Error", "Payment Auth - Error: " + result.ErrorMessage, true);
                    return result;
                }

                payment.ProviderTransactionID = transactionId;
                payment.SetMetaField(NetaxeptConstants.CardInformationPaymentMethodField, paymentResult.CardInformationPaymentMethod, false);
                payment.SetMetaField(NetaxeptConstants.CardInformationExpiryDateField, paymentResult.CardInformationExpiryDate, false);
                payment.SetMetaField(NetaxeptConstants.CardInformationIssuerCountryField, paymentResult.CardInformationIssuerCountry, false);
                payment.SetMetaField(NetaxeptConstants.CardInformationIssuerField, paymentResult.CardInformationIssuer, false);
                payment.SetMetaField(NetaxeptConstants.CardInformationIssuerIdField, paymentResult.CardInformationIssuerId, false);
                payment.SetMetaField(NetaxeptConstants.CardInformationMaskedPanField, paymentResult.CardInformationMaskedPan, false);
                
                // Save the PanHash(if not empty) on the customer contact, so we can use EasyPayment for next payment
                if (!string.IsNullOrEmpty(paymentResult.CardInformationPanHash) &&
                    orderForm.Parent.CustomerId != Guid.Empty)
                {
                    var customerContact = CustomerContext.Current.GetContactById(orderForm.Parent.CustomerId);
                    if (customerContact != null)
                    {
                        customerContact[NetaxeptConstants.CustomerPanHashFieldName] =
                            paymentResult.CardInformationPanHash;
                        customerContact.SaveChanges();
                    }
                }
                result.Result = PaymentResponseCode.Success;

                AddNote(payment.Parent, "Payment - Auth", "Payment - Auth");
            }
            return result;
        }
    }

}

using System;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
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

        public AuthorizationPaymentStep(IPayment payment) : base(payment)
        { }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="payment"></param>
       /// <param name="orderForm"></param>
       /// <param name="orderGroup"></param>
       /// <param name="message"></param>
       /// <returns></returns>
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Process payment
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="orderGroup"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public PaymentAuthorizationResult Process(IPayment payment, IOrderGroup orderGroup, string transactionId)
        {
            var result = new PaymentAuthorizationResult();
            if (payment.TransactionType == "Authorization" && !string.IsNullOrEmpty(transactionId))
            {
                PaymentResult paymentResult = null;
                try
                {
                    paymentResult = this.Client.Query(transactionId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);

                    AddNoteAndSaveChanges(orderGroup, "Payment Query - Error", "Payment Query - Error: " + ex.Message);
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

                    AddNoteAndSaveChanges(orderGroup, "Payment - Failed", "Payment - Error: " + result.ErrorMessage);
                    return result;
                }

                try
                {
                    var responseResult = this.Client.Authorize(transactionId);
                    if (responseResult.ErrorOccurred)
                    {
                        payment.Status = "Failed";
                        result.Result = PaymentResponseCode.ErrorOccurred;
                        AddNoteAndSaveChanges(orderGroup, "Payment Auth - Error", "Payment Auth - Error: " + responseResult.ErrorMessage);

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    payment.Status = "Failed";
                    result.Result = PaymentResponseCode.ErrorOccurred;
                    result.ErrorMessage = ex.Message;

                    Logger.Error(ex.Message);

                    AddNoteAndSaveChanges(orderGroup, "Payment Auth - Error", "Payment Auth - Error: " + result.ErrorMessage);
                    return result;
                }

                payment.TransactionID = transactionId;
                payment.ProviderTransactionID = transactionId;
                payment.Properties[NetaxeptConstants.CardInformationPaymentMethodField] = paymentResult.CardInformationPaymentMethod;
                payment.Properties[NetaxeptConstants.CardInformationExpiryDateField] = paymentResult.CardInformationExpiryDate;
                payment.Properties[NetaxeptConstants.CardInformationIssuerCountryField] = paymentResult.CardInformationIssuerCountry;
                payment.Properties[NetaxeptConstants.CardInformationIssuerField] = paymentResult.CardInformationIssuer;
                payment.Properties[NetaxeptConstants.CardInformationIssuerIdField] = paymentResult.CardInformationIssuerId;
                payment.Properties[NetaxeptConstants.CardInformationMaskedPanField] = paymentResult.CardInformationMaskedPan;
                
                // Save the PanHash(if not empty) on the customer contact, so we can use EasyPayment for next payment
                if (!string.IsNullOrEmpty(paymentResult.CardInformationPanHash) && orderGroup.CustomerId != Guid.Empty)
                {
                    var customerContact = CustomerContext.Current.GetContactById(orderGroup.CustomerId);
                    if (customerContact != null)
                    {
                        customerContact[NetaxeptConstants.CustomerPanHashFieldName] = paymentResult.CardInformationPanHash;
                        customerContact[NetaxeptConstants.CustomerCardMaskedFieldName] = paymentResult.CardInformationMaskedPan;
                        customerContact[NetaxeptConstants.CustomerCardExpirationDateFieldName] = paymentResult.CardInformationExpiryDate;
                        customerContact[NetaxeptConstants.CustomerCardPaymentMethodFieldName] = paymentResult.CardInformationPaymentMethod;
                        customerContact[NetaxeptConstants.CustomerCardIssuerCountryFieldName] = paymentResult.CardInformationIssuerCountry;
                        customerContact[NetaxeptConstants.CustomerCardIssuerIdFieldName] = paymentResult.CardInformationIssuerId;
                        customerContact[NetaxeptConstants.CustomerCardIssuerFieldName] = paymentResult.CardInformationIssuer;

                        customerContact.SaveChanges();
                    }
                }
                result.Result = PaymentResponseCode.Success;

                AddNoteAndSaveChanges(orderGroup, "Payment - Auth", "Payment - Auth");

            }
            return result;
        }
        
    }

}

using System;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment should authenticated at Netaxept
    /// </summary>
    public class AuthenticatePaymentStep : PaymentStep
    {
        public AuthenticatePaymentStep(Payment payment) : base(payment)
        { }

        /// <summary>
        /// Process payment
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(Payment payment, ref string message)
        {
            var transactionIdResult = PaymentStepHelper.GetTransactionFromCookie<string>(NetaxeptConstants.PaymentResultCookieName);

            if (payment.TransactionType == "Authorization" && !string.IsNullOrEmpty(transactionIdResult))
            {
                var orderForm = payment.Parent;

                var paymentResult = this.Client.Query(transactionIdResult);

                if (paymentResult.AmountCaptured > 0)
                {
                    return true;
                }

                if (paymentResult.Cancelled)
                {
                    message = "The payment was cancelled by the user.";
                    PaymentStepHelper.SaveTransactionToCookie(null, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                    return false;
                }
                if (paymentResult.ErrorOccurred)
                {
                    message = paymentResult.ErrorMessage;
                    PaymentStepHelper.SaveTransactionToCookie(null, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                    return false;
                }

                //netaxeptServiceClient.Sale(merchantId, token, transactionIdResult);
                // Don't call sale but auth instead
                this.Client.Authorize(transactionIdResult);

                payment.ProviderTransactionID = transactionIdResult;
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
                PaymentStepHelper.SaveTransactionToCookie(null, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));

                //AddNote(orderForm, "Payment - Authenticated", "Payment - Amount is authenticated");

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

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
        /// <summary>
        /// Process payment
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(Payment payment, ref string message)
        {
            var transactionIdResult = PaymentStepHelper.GetTransactionFromCookie<string>(NetaxeptConstants.PaymentResultCookieName);

            if (!string.IsNullOrEmpty(transactionIdResult))
            {
                var orderForm = payment.Parent;
                var paymentMethoDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
                var merchantId = paymentMethoDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
                var token = paymentMethoDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);

                var paymentResult = this.Client.Query(merchantId, token, transactionIdResult);

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
                this.Client.Authorize(merchantId, token, transactionIdResult);

                payment.ProviderTransactionID = transactionIdResult;

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

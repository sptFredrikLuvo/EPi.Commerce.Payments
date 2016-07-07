using System;
using EPiServer.Logging;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Website;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    /// <summary>
    /// Netaxept payment gateway
    /// </summary>
    public class NetaxeptCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentOption
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(NetaxeptCheckoutPaymentGateway));

        public Guid PaymentMethodId { get; set; }
        public string SuccessUrl { get; set; }
        public string CallBackUrlWhenFail { get; set; }

        /// <summary>
        /// Process payment method 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            try
            {
                Logger.Debug("Netaxept checkout gateway. Processing Payment ....");

                var registerPaymentStep = new RegisterPaymentStep(payment);
                var authenticatePaymentStep = new AuthenticatePaymentStep(payment);
                var capturePaymentStep = new CapturePaymentStep(payment);
                var creditPaymentStep = new CreditPaymentStep(payment);

                registerPaymentStep.SetSuccessor(authenticatePaymentStep);
                capturePaymentStep.SetSuccessor(registerPaymentStep);
                creditPaymentStep.SetSuccessor(capturePaymentStep);

                return creditPaymentStep.Process(payment, ref message);
            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                message = exception.Message;
                throw;
            }
        }

        public bool ValidateData()
        {
            return true;
        }

        /// <summary>
        /// Pre process method. Set all fields for the payment
        /// </summary>
        /// <param name="orderForm"></param>
        /// <returns></returns>
        public Payment PreProcess(OrderForm orderForm)
        {
            if (orderForm == null) throw new ArgumentNullException(nameof(orderForm));

            var payment = new NetaxeptPayment()
            {
                PaymentMethodId = PaymentMethodId,
                PaymentMethodName = "NetaxeptCheckout",
                OrderFormId = orderForm.OrderFormId,
                OrderGroupId = orderForm.OrderGroupId,
                Amount = orderForm.Total,
                Status = PaymentStatus.Pending.ToString(),
                TransactionType = TransactionType.Authorization.ToString()
            };

            payment.SetMetaField(NetaxeptConstants.SuccessfullUrl, SuccessUrl, false);

            return payment;
        }

        /// <summary>
        /// Will always return true
        /// </summary>
        /// <param name="orderForm"></param>
        /// <returns></returns>
        public bool PostProcess(OrderForm orderForm)
        {
            return true;
        }

        public bool ProcessSuccessfulTransaction(Payment payment, string transactionId)
        {
            if (payment.TransactionType == "Authorization" && !string.IsNullOrEmpty(transactionId))
            {
                var orderForm = payment.Parent;

                var paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
                var merchantId = paymentMethodDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
                var token = paymentMethodDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);

                var connection = new ClientConnection(merchantId, token);
                var client = new NetaxeptServiceClient(connection);

                var paymentResult = client.Query(transactionId);

                if (paymentResult.Cancelled)
                {
                    payment.Status = "Failed";
                    return false;
                }
                if (paymentResult.ErrorOccurred)
                {
                    payment.Status = "Failed";
                    return false;
                }
                
                client.Authorize(transactionId);

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
                return true;
            }
            return false;
        }
    }
}

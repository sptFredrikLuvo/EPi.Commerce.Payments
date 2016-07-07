using System;
using EPiServer.Logging;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Models;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;
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
                var capturePaymentStep = new CapturePaymentStep(payment);
                var creditPaymentStep = new CreditPaymentStep(payment);

                registerPaymentStep.SetSuccessor(capturePaymentStep);
                capturePaymentStep.SetSuccessor(creditPaymentStep);

                return registerPaymentStep.Process(payment, ref message);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public PaymentAuthorizationResult ProcessAuthorization(Payment payment, string transactionId)
        {
            var authorizePaymentStep = new AuthorizationPaymentStep(payment);

            return authorizePaymentStep.Process(payment, transactionId);
        }
    }
}

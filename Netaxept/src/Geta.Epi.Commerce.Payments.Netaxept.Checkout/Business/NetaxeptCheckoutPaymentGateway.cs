using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Models;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Website;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    /// <summary>
    /// Netaxept payment gateway
    /// </summary>
    public class NetaxeptCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(NetaxeptCheckoutPaymentGateway));

        public Guid PaymentMethodId { get; set; }
        public string CallbackUrl { get; set; }

        public IOrderGroup OrderGroup { get; set; }
        private IOrderForm _orderForm;

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            return ProcessPayment(OrderGroup, payment).IsSuccessful;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public PaymentAuthorizationResult ProcessAuthorization(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, string transactionId)
        {
            var authorizePaymentStep = new AuthorizationPaymentStep(payment);

            return authorizePaymentStep.Process(payment, orderGroup, transactionId);
        }

        public PaymentResult QueryTransaction(Guid paymentMethodId, string transactionId)
        {
            try
            {
                var paymentMethodDto = PaymentManager.GetPaymentMethod(paymentMethodId);
                var merchantId = paymentMethodDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
                var token = paymentMethodDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);
                var isProduction = bool.Parse(paymentMethodDto.GetParameter(NetaxeptConstants.IsProductionField, "false"));

                var connection = new ClientConnection(merchantId, token, isProduction);
                var client = new NetaxeptServiceClient(connection);
                return client.Query(transactionId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return null;
        }


        /// <summary>
        /// Process payment method 
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            OrderGroup = payment.Parent.Parent;
            _orderForm = payment.Parent;
            return ProcessPayment(OrderGroup, payment).IsSuccessful;
        }

        public virtual PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            PaymentStepResult stepResult;

            try
            {
                Logger.Debug("Netaxept checkout gateway. Processing Payment ....");

                if (_orderForm == null)
                {
                    _orderForm = orderGroup.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
                }

                var registerPaymentStep = new RegisterPaymentStep(payment);
                var capturePaymentStep = new CapturePaymentStep(payment);
                var creditPaymentStep = new CreditPaymentStep(payment);
                var annulPaymentStep = new AnnulPaymentStep(payment);

                registerPaymentStep.SetSuccessor(capturePaymentStep);
                capturePaymentStep.SetSuccessor(creditPaymentStep);
                creditPaymentStep.SetSuccessor(annulPaymentStep);

                stepResult = registerPaymentStep.Process(payment, _orderForm, orderGroup);
            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                stepResult = new PaymentStepResult { IsSuccessful = false, Message = exception.Message };
            }

            if (stepResult.IsSuccessful)
            {
                return PaymentProcessingResult.CreateSuccessfulResult(stepResult.Message, stepResult.RedirectUrl);
            }

            return PaymentProcessingResult.CreateUnsuccessfulResult(stepResult.Message);
        }
    }
}

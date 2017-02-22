using System;
using System.Linq;
using EPiServer.Commerce.Order;
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
    public class NetaxeptCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(NetaxeptCheckoutPaymentGateway));

        public Guid PaymentMethodId { get; set; }
        public string CallbackUrl { get; set; }

        public IOrderGroup OrderGroup { get; set; }
        private IOrderForm _orderForm;

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            try
            {
                Logger.Debug("Netaxept checkout gateway. Processing Payment ....");

                if (_orderForm == null)
                {
                    _orderForm = OrderGroup.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
                }

                var registerPaymentStep = new RegisterPaymentStep(payment);
                var capturePaymentStep = new CapturePaymentStep(payment);
                var creditPaymentStep = new CreditPaymentStep(payment);
                var annulPaymentStep = new AnnulPaymentStep(payment);

                registerPaymentStep.SetSuccessor(capturePaymentStep);
                capturePaymentStep.SetSuccessor(creditPaymentStep);
                creditPaymentStep.SetSuccessor(annulPaymentStep);

                return registerPaymentStep.Process(payment, _orderForm, OrderGroup, ref message);
            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                message = exception.Message;
                throw;
            }
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

                OrderGroup = payment.Parent.Parent;
                _orderForm = payment.Parent;
                return ProcessPayment(payment as IPayment, ref message);
            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                message = exception.Message;
                throw;
            }
        }
    }
}

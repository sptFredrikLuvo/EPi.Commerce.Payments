using System;
using EPiServer.Security;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment step 
    /// </summary>
    public abstract class PaymentStep
    {
        protected NetaxeptServiceClient Client;
        protected PaymentStep Successor;

        protected PaymentStep(Payment payment)
        {
            var paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            var merchantId = paymentMethodDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
            var token = paymentMethodDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);

            var connection = new ClientConnection(merchantId, token);
            Client = new NetaxeptServiceClient(connection);
        }

        public void SetSuccessor(PaymentStep successor)
        {
            this.Successor = successor;
        }

        /// <summary>
        /// Process payment step
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool Process(Payment payment, ref string message);

        /// <summary>
        /// Add note to order
        /// </summary>
        /// <param name="orderForm"></param>
        /// <param name="title"></param>
        /// <param name="detail"></param>
        public void AddNote(OrderForm orderForm, string title, string detail)
        {
            OrderNote on = orderForm.Parent.OrderNotes.AddNew();
            on.Detail = detail;
            on.Title = title;
            on.Type = OrderNoteTypes.Custom.ToString();
            on.Created = DateTime.Now;
            on.CustomerId = PrincipalInfo.CurrentPrincipal.GetContactId();
        }
    }
}

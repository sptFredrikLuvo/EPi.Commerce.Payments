using System;
using EPiServer.Commerce.Order;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Customers;
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
        private readonly Injected<IOrderRepository> _orderRepository;
        private readonly Injected<IOrderGroupFactory> _orderGroupFactory;

        /// <summary>
        /// Constructor. Get the merchantId and token and create a connection object
        /// </summary>
        /// <param name="payment"></param>
        protected PaymentStep(IPayment payment)
        {
            var paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            var merchantId = paymentMethodDto.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
            var token = paymentMethodDto.GetParameter(NetaxeptConstants.TokenField, string.Empty);
            var isProduction = bool.Parse(paymentMethodDto.GetParameter(NetaxeptConstants.IsProductionField, "false"));

            var connection = new ClientConnection(merchantId, token, isProduction);
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
        /// <param name="orderForm"></param>
        /// <param name="orderGroup"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract PaymentStepResult Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup);
        
        protected void AddNoteAndSaveChanges(IOrderGroup orderGroup, string noteTitle, string noteMessage)
        {
            var note = _orderGroupFactory.Service.CreateOrderNote(orderGroup);
            note.CustomerId = CustomerContext.Current.CurrentContactId;
            note.Type = OrderNoteTypes.Custom.ToString();
            note.Title = noteTitle;
            note.Detail = noteMessage;
            note.Created = DateTime.UtcNow;
            orderGroup.Notes.Add(note);
            _orderRepository.Service.Save(orderGroup);
        }

        protected PaymentStepResult Success(string redirectUrl)
        {
            return new PaymentStepResult
            {
                IsSuccessful = true,
                RedirectUrl = redirectUrl
            };
        }

        protected PaymentStepResult Success()
        {
            return Success(null);
        }

        protected PaymentStepResult Fail(string message)
        {
            return new PaymentStepResult
            {
                IsSuccessful = false,
                Message = message
            };
        }
    }
}

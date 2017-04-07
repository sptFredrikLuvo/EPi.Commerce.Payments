using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Website.Helpers;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class PaymentCallbackController : Controller
    {
        private CheckoutService _checkoutService;
        private CustomerContextFacade _customerContext;
        private ICartService _cartService;
        private IOrderRepository _orderRepository;

        public RedirectResult Index(string transactionId)
        {
            _checkoutService = ServiceLocator.Current.GetInstance<CheckoutService>();
            _customerContext = ServiceLocator.Current.GetInstance<CustomerContextFacade>();
            _cartService = ServiceLocator.Current.GetInstance<ICartService>();
            _orderRepository = ServiceLocator.Current.GetInstance<IOrderRepository>();
            
            var cart = _cartService.LoadCart(_cartService.DefaultCartName);

            var payment = GetPayment(cart);

            var netaxeptCheckoutPaymentGateway = new NetaxeptCheckoutPaymentGateway();

            var result = netaxeptCheckoutPaymentGateway.ProcessAuthorization(payment, cart.GetFirstForm(), cart, transactionId);
            if (result.Result == PaymentResponseCode.Success)
            {
                var orderLink = _orderRepository.SaveAsPurchaseOrder(cart);

                var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderLink.OrderGroupId);

                // this will copy all notes from the Cart to the PurchaseOrder
                //CopyNotesFromCartToPurchaseOrder(purchaseOrder, cart);
                
                _orderRepository.Delete(cart.OrderLink);

                var queryCollection = new NameValueCollection
                {
                    {"contactId", _customerContext.CurrentContactId.ToString()},
                    {"orderNumber", purchaseOrder.OrderNumber.ToString(CultureInfo.InvariantCulture)}
                };
                return new RedirectResult(new UrlBuilder("/checkout/order-confirmation/") { QueryCollection = queryCollection }.ToString());
            }
            return new RedirectResult(new UrlBuilder("/error-pages/payment-failed/").ToString());
        }

        /// <summary>
        /// Copy notes from cart to purchse order
        /// </summary>
        /// <param name="purchaseOrder"></param>
        /// <param name="cart"></param>
        private void CopyNotesFromCartToPurchaseOrder(PurchaseOrder purchaseOrder, Mediachase.Commerce.Orders.Cart cart)
        {
            foreach (var note in cart.OrderNotes.OrderByDescending(n => n.Created))
            {
                OrderNote on = purchaseOrder.OrderNotes.AddNew();
                on.Detail = note.Detail;
                on.Title = note.Title;
                on.Type = OrderNoteTypes.System.ToString();
                on.Created = note.Created;
                on.CustomerId = note.CustomerId;
            }
            purchaseOrder.AcceptChanges();
        }

        /// <summary>
        /// Get payment
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        private IPayment GetPayment(ICart cart)
        {
            if (cart.Forms == null || cart.Forms.Count == 0 || cart.GetFirstForm().Payments == null || cart.GetFirstForm().Payments.Count == 0)
                return null;

            var payments = cart.GetFirstForm().Payments.Where(p => p.Status != PaymentStatus.Failed.ToString()).ToList();

            if (payments.Any())
                return payments.FirstOrDefault(x => x.TransactionType.Equals(TransactionType.Authorization.ToString()));
            return null;
        }
    }
}
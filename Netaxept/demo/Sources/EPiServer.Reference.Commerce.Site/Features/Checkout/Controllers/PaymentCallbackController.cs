using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
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
        private ICheckoutService _checkoutService;
        private CustomerContextFacade _customerContext;

        public RedirectResult Index(string transactionId)
        {
            _checkoutService = ServiceLocator.Current.GetInstance<ICheckoutService>();
            _customerContext = ServiceLocator.Current.GetInstance<CustomerContextFacade>();

            PurchaseOrder purchaseOrder = null;

            Mediachase.Commerce.Orders.Cart cart = new CartHelper(Mediachase.Commerce.Orders.Cart.DefaultName).Cart;
            
            var payment = GetPayment(cart);

            var netaxeptCheckoutPaymentGateway = new NetaxeptCheckoutPaymentGateway();

            var result = netaxeptCheckoutPaymentGateway.ProcessAuthorization(payment, transactionId);
            if (result.Result == PaymentResponseCode.Success)
            {
                purchaseOrder = _checkoutService.SaveCartAsPurchaseOrder();

                // this will copy all notes from the Cart to the PurchaseOrder
                CopyNotesFromCartToPurchaseOrder(purchaseOrder, cart); 

                _checkoutService.DeleteCart();

                var queryCollection = new NameValueCollection
                {
                    {"contactId", _customerContext.CurrentContactId.ToString()},
                    {"orderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
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
        private Mediachase.Commerce.Orders.Payment GetPayment(Mediachase.Commerce.Orders.Cart cart)
        {
            if (cart.OrderForms == null || cart.OrderForms.Count == 0 || cart.OrderForms[0].Payments == null || cart.OrderForms[0].Payments.Count == 0)
                return null;

            List<Mediachase.Commerce.Orders.Payment> payments = cart.OrderForms[0].Payments.Where(p => p.Status != PaymentStatus.Failed.ToString()).ToList();
            payments = PaymentTransactionTypeManager.GetResultingPaymentsByTransactionType(payments, TransactionType.Authorization).ToList();

            if (payments.Any())
                return payments.First();
            return null;
        }
    }
}

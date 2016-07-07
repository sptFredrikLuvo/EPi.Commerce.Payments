using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business;

using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class PaymentCallbackController : Controller
    {
        private ICheckoutService _checkoutService;
        private CustomerContextFacade _customerContext;

        public RedirectResult Index(string orderNumber, string transactionId)
        {
            _checkoutService = ServiceLocator.Current.GetInstance<ICheckoutService>();
            _customerContext = ServiceLocator.Current.GetInstance<CustomerContextFacade>();

            PurchaseOrder purchaseOrder = null;

            Mediachase.Commerce.Orders.Cart cart = new CartHelper(Mediachase.Commerce.Orders.Cart.DefaultName).Cart;
            var payment = GetPayment(cart);

            var netaxeptCheckoutPaymentGateway = new NetaxeptCheckoutPaymentGateway();

            var result = netaxeptCheckoutPaymentGateway.ProcessSuccessfulTransaction(payment, transactionId);
            if (result)
            {
                purchaseOrder = _checkoutService.SaveCartAsPurchaseOrder();
                _checkoutService.DeleteCart();

                var queryCollection = new NameValueCollection
                {
                    {"contactId", _customerContext.CurrentContactId.ToString()},
                    {"orderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
                };

                return new RedirectResult(new UrlBuilder("") { QueryCollection = queryCollection }.ToString());
            }
            //return new RedirectResult(new UrlBuilder(confirmationPage.LinkURL) { QueryCollection = queryCollection }.ToString()););
            return null;
        }

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

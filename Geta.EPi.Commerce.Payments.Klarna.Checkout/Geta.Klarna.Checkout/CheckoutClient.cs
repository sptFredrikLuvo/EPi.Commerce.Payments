using System;
using System.Collections.Generic;
using Geta.Klarna.Checkout.Extensions;
using Klarna.Checkout;

namespace Geta.Klarna.Checkout
{
    public interface ICheckoutClient
    {
        CheckoutResponse Checkout(IEnumerable<ICartItem> cartItems, Locale locale, CheckoutUris checkoutUris);
        ConfirmResponse Confirm(Uri location);
        void Acknowledge(Uri location);
    }

    public class CheckoutClient : ICheckoutClient
    {
        public Uri OrderBaseUri { get; private set; }
        public string MerchantId { get; private set; }
        public string SharedSecret { get; private set; }
        public bool AllowSeparateShippingAddress { get; private set; }

        const string ContentType = "application/vnd.klarna.checkout.aggregated-order-v2+json";

        public CheckoutClient(Uri orderBaseUri, string merchantId, string sharedSecret, bool allowSeparateShippingAddress = false)
        {
            if (orderBaseUri == null) throw new ArgumentNullException("orderBaseUri");
            if (merchantId == null) throw new ArgumentNullException("merchantId");
            if (sharedSecret == null) throw new ArgumentNullException("sharedSecret");
            OrderBaseUri = orderBaseUri;
            MerchantId = merchantId;
            SharedSecret = sharedSecret;
            AllowSeparateShippingAddress = allowSeparateShippingAddress;
        }

        public CheckoutResponse Checkout(IEnumerable<ICartItem> cartItems, Locale locale, CheckoutUris checkoutUris)
        {
            var connector = Connector.Create(SharedSecret);

            var merchant = new Merchant(
                MerchantId, 
                SharedSecret, 
                checkoutUris.Checkout, 
                checkoutUris.Confirmation, 
                checkoutUris.Push, 
                checkoutUris.Terms);
            var cart = new Cart(cartItems);
            var options = new Options(AllowSeparateShippingAddress);
            var data = new OrderData(merchant, cart, locale, options);
            var order = new Order(connector)
            {
                BaseUri = OrderBaseUri,
                ContentType = ContentType
            };
            order.Create(data.ToDictionary());
            order.Fetch();

            return new CheckoutResponse(order.Location, order.GetSnippet());
        }

        public ConfirmResponse Confirm(Uri location)
        {
            var order = FetchOrder(location);
            var snippet = order.GetSnippet();
            var billingAddress = order.GetBillingAddress();
            var shippingAddress = order.GetShippingAddress();
            MarkOrderCreatedOnComplete(order);

            return new ConfirmResponse(location, snippet, billingAddress, shippingAddress);
        }

        public void Acknowledge(Uri location)
        {
            var order = FetchOrder(location);
            MarkOrderCreatedOnComplete(order);
        }

        private Order FetchOrder(Uri location)
        {
            var connector = Connector.Create(SharedSecret);
            var order = new Order(connector, location)
            {
                ContentType = ContentType
            };

            order.Fetch();
            return order;
        }

        private static void MarkOrderCreatedOnComplete(Order order)
        {
            if ((string) order.GetValue("status") != "checkout_complete") return;

            var data = new Dictionary<string, object>
            {
                {"status", "created"}
            };

            order.Update(data);
        }
    }
}
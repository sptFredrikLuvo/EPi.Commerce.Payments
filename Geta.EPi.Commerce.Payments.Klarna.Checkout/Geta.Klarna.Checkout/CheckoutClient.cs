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
        ConfirmResponse Confirm(Uri location, MerchantReference merchantReference);
        void Acknowledge(Uri location);
        void Acknowledge(Uri location, MerchantReference merchantReference);
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
            if (cartItems == null) throw new ArgumentNullException("cartItems");
            if (locale == null) throw new ArgumentNullException("locale");
            if (checkoutUris == null) throw new ArgumentNullException("checkoutUris");

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
            return Confirm(location, MerchantReference.Empty);
        }

        public ConfirmResponse Confirm(Uri location, MerchantReference merchantReference)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (merchantReference == null) throw new ArgumentNullException("merchantReference");

            var order = FetchOrder(location);
            var snippet = order.GetSnippet();
            var billingAddress = order.GetBillingAddress();
            var shippingAddress = order.GetShippingAddress();
            order.Confirm(merchantReference);

            return new ConfirmResponse(location, snippet, billingAddress, shippingAddress);
        }

        public void Acknowledge(Uri location)
        {
            Acknowledge(location, MerchantReference.Empty);
        }

        public void Acknowledge(Uri location, MerchantReference merchantReference)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (merchantReference == null) throw new ArgumentNullException("merchantReference");

            var order = FetchOrder(location);
            order.Confirm(merchantReference);
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
    }
}
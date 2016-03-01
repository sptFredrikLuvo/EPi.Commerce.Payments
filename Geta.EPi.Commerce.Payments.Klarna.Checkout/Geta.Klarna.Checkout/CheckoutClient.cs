using System;
using System.Collections.Generic;
using Geta.Klarna.Checkout.Extensions;
using Geta.Klarna.Checkout.Models;
using Klarna.Checkout;

namespace Geta.Klarna.Checkout
{
    public interface ICheckoutClient
    {
        CheckoutResponse Checkout(IEnumerable<ICartItem> cartItems, Locale locale, CheckoutUris checkoutUris, ShippingAddress address = null);
        ConfirmResponse Confirm(string orderId);
        ConfirmResponse Confirm(string orderId, MerchantReference merchantReference);
        OrderResponse GetOrder(string orderId);
        bool UpdateOrderId(string orderId, string commerceOrderId);
        void Acknowledge(string orderId);
        void Acknowledge(string orderId, MerchantReference merchantReference);
    }

    public class CheckoutClient : ICheckoutClient
    {
        public Uri OrderBaseUri { get; private set; }
        public string MerchantId { get; private set; }
        public string SharedSecret { get; private set; }
        public bool AllowSeparateShippingAddress { get; private set; }
        public string CustomColor { get; set; }

        const string ContentType = "application/vnd.klarna.checkout.aggregated-order-v2+json";

        public CheckoutClient(Uri orderBaseUri, string merchantId, string sharedSecret, bool allowSeparateShippingAddress = false, string customColorCode = null)
        {
            if (orderBaseUri == null) throw new ArgumentNullException("orderBaseUri");
            if (merchantId == null) throw new ArgumentNullException("merchantId");
            if (sharedSecret == null) throw new ArgumentNullException("sharedSecret");
            OrderBaseUri = orderBaseUri;
            MerchantId = merchantId;
            SharedSecret = sharedSecret;
            AllowSeparateShippingAddress = allowSeparateShippingAddress;
            CustomColor = customColorCode;
        }

        public CheckoutResponse Checkout(IEnumerable<ICartItem> cartItems, Locale locale, CheckoutUris checkoutUris, ShippingAddress address = null)
        {
            if (cartItems == null) throw new ArgumentNullException("cartItems");
            if (locale == null) throw new ArgumentNullException("locale");
            if (checkoutUris == null) throw new ArgumentNullException("checkoutUris");

            var connector = Connector.Create(SharedSecret, OrderBaseUri);

            var merchant = new Merchant(
                MerchantId, 
                SharedSecret, 
                checkoutUris.Checkout, 
                checkoutUris.Confirmation, 
                checkoutUris.Push, 
                checkoutUris.Terms,
                checkoutUris.Validation);
            var cart = new Cart(cartItems);
            var options = new Options(AllowSeparateShippingAddress);
            if (!string.IsNullOrEmpty(CustomColor))
                options.ButtonColorCode = CustomColor;
            var data = new OrderData(merchant, cart, locale, options, address);
            var order = new Order(connector)
            {
                ContentType = ContentType
            };
            order.Create(data.ToDictionary());
            order.Fetch();

            return new CheckoutResponse(order.GetStringField("id"), order.Location, order.GetSnippet(), order.GetStringField("status"));
        }

        public ConfirmResponse Confirm(string orderId)
        {
            return Confirm(orderId, MerchantReference.Empty);
        }

        public ConfirmResponse Confirm(string orderId, MerchantReference merchantReference)
        {
            if (orderId == null) throw new ArgumentNullException("orderId");
            if (merchantReference == null) throw new ArgumentNullException("merchantReference");

            var order = FetchOrder(orderId);
            var snippet = order.GetSnippet();
            var billingAddress = order.GetBillingAddress();
            var shippingAddress = order.GetShippingAddress();
            order.Confirm(merchantReference);

            return new ConfirmResponse(orderId, snippet, billingAddress, shippingAddress, order.GetStringField("status"), order.GetStringField("reservation"), order.GetTotalCost());
        }

        public OrderResponse GetOrder(string orderId)
        {
            if (orderId == null) throw new ArgumentNullException("orderId");
            var order = FetchOrder(orderId);
            return new OrderResponse(order.GetSnippet(), order.GetTotalCost());
        }

        public bool UpdateOrderId(string orderId, string commerceOrderId)
        {
            if (orderId == null) throw new ArgumentNullException("orderId");
            if (commerceOrderId == null) throw new ArgumentNullException("commerceOrderId");

            var order = FetchOrder(orderId);

            var merchant = new MerchantReference(commerceOrderId, string.Empty);

            var data = new Dictionary<string, object>
            {
                {"merchant_reference", merchant.ToDictionary()}
            };

            order.Update(data);
            return true;
        }

        public void Acknowledge(string orderId)
        {
            Acknowledge(orderId, MerchantReference.Empty);
        }

        public void Acknowledge(string orderId, MerchantReference merchantReference)
        {
            if (orderId == null) throw new ArgumentNullException("orderId");
            if (merchantReference == null) throw new ArgumentNullException("merchantReference");

            var order = FetchOrder(orderId);
            order.Confirm(merchantReference);
        }

        private Order FetchOrder(string orderId)
        {
            var connector = Connector.Create(SharedSecret, OrderBaseUri);
            var order = new Order(connector, orderId)
            {
                ContentType = ContentType
            };

            order.Fetch();
            return order;
        }
    }
}
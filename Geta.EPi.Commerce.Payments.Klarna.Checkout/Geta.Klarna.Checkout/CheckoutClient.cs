using System;
using System.Collections.Generic;
using Geta.Klarna.Checkout.Extensions;
using Geta.Klarna.Checkout.Models;
using Klarna.Checkout;

namespace Geta.Klarna.Checkout
{
    public interface ICheckoutClient
    {
        CheckoutResponse Checkout(IEnumerable<ICartItem> cartItems, Locale locale, CheckoutUris checkoutUris, string orderId = null, ShippingAddress address = null);
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
        public ColorOptions ColorOptions { get; private set; }
        public bool DisableAutoFocus { get; private set; }
        public bool EnableOrganizationCheckout { get; set; }

        const string ContentType = "application/vnd.klarna.checkout.aggregated-order-v2+json";

        public CheckoutClient(Uri orderBaseUri, string merchantId, string sharedSecret, bool allowSeparateShippingAddress = false, ColorOptions colorOptions = null, bool disableAutoFocus = false, bool enableOrganizationCheckout = false)
        {
            if (orderBaseUri == null) throw new ArgumentNullException("orderBaseUri");
            if (merchantId == null) throw new ArgumentNullException("merchantId");
            if (sharedSecret == null) throw new ArgumentNullException("sharedSecret");
            OrderBaseUri = orderBaseUri;
            MerchantId = merchantId;
            SharedSecret = sharedSecret;
            AllowSeparateShippingAddress = allowSeparateShippingAddress;
            ColorOptions = colorOptions;
            DisableAutoFocus = disableAutoFocus;
            EnableOrganizationCheckout = enableOrganizationCheckout;
        }

        public CheckoutResponse Checkout(IEnumerable<ICartItem> cartItems, Locale locale, CheckoutUris checkoutUris, string orderId = null, ShippingAddress address = null)
        {
            if (cartItems == null) throw new ArgumentNullException("cartItems");
            if (locale == null) throw new ArgumentNullException("locale");
            if (checkoutUris == null) throw new ArgumentNullException("checkoutUris");

            var connector = Connector.Create(SharedSecret, OrderBaseUri);
            Order order = null;

            bool orderExists = string.IsNullOrEmpty(orderId) == false;

            if (orderExists)
            {
                try
                {
                    // try to get existing order
                    order = new Order(connector, orderId)
                    {
                        ContentType = ContentType
                    };

                    order.Fetch();
                }
                catch
                {
                    //throws exception if cannot find order - we'll create a new one
                    orderExists = false;
                }
            }

            if (orderExists == false)
            {
                var merchant = new Merchant(
                    MerchantId,
                    SharedSecret,
                    checkoutUris.Checkout,
                    checkoutUris.Confirmation,
                    checkoutUris.Push,
                    checkoutUris.Terms,
                    checkoutUris.Validation);
                var cart = new Cart(cartItems);
                var options = new Options(AllowSeparateShippingAddress, EnableOrganizationCheckout);
                var gui = new Gui(DisableAutoFocus);
                if (ColorOptions != null)
                    options.ColorOptions = ColorOptions;
                var data = new OrderData(merchant, cart, locale, options, gui, address);


                order = new Order(connector)
                {
                    ContentType = ContentType
                };
                order.Create(data.ToDictionary());
                order.Fetch();
            }
            else
            {
                // updating cart will only work for order with status "checkout_incomplete"
                var cart = new Cart(cartItems);
                var updateData = new Dictionary<string, object> { { "cart", cart.ToDictionary() } };

                var status = order.GetStringField("status");

                if (!status.Equals(OrderStatus.InComplete, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Cannot change order that has status " + status);

                order.Update(updateData);
            }

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
            return new OrderResponse(order.GetSnippet(), order.GetTotalCost(), order.GetCustomerName(), order.GetShippingAddress(), order.GetBillingAddress());
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
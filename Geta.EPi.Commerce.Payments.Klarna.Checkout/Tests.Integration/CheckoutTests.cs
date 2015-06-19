using System.Collections.Generic;
using FluentAssertions;
using Geta.Klarna.Checkout;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Tests.Integration
{
    public class CheckoutTests
    {

        // Only first step can be tested, because to get to the second step, 
        // manual user action should be done through rendered snippet which received from Klarna in first step.

        [Theory]
        [AutoData]
        public void it_gets_checkout_snippet(
            List<CartItem> cartItems,
            ShippingItem shippingItem,
            CheckoutUris checkoutUris)
        {
            var sut = new CheckoutClient(TestConfig.OrderBaseUri, TestConfig.MerchantId, TestConfig.SharedSecret);
            var items = new List<ICartItem>();
            items.AddRange(cartItems);
            items.Add(shippingItem);

            var response = sut.Checkout(items, Locale.Norway, checkoutUris);
            response.Snippet.Should().NotBeNullOrWhiteSpace();
            response.Location.AbsoluteUri.Should().StartWith(TestConfig.OrderBaseUri.ToString());

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Geta.Klarna.Checkout;
using Geta.Klarna.Checkout.Models;
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
            ShippingItem shippingItem)
        {
            var sut = new CheckoutClient(TestConfig.OrderBaseUri, TestConfig.MerchantId, TestConfig.SharedSecret);
            var items = new List<ICartItem>();
            items.AddRange(cartItems);
            items.Add(shippingItem);

            var testUrl = "http://www.mysite.com";

            var checkoutUris = new CheckoutUris(
                 new Uri(testUrl), new Uri(testUrl), new Uri(testUrl), new Uri(testUrl));

            var response = sut.Checkout(items, Locale.Norway, checkoutUris);
            response.Snippet.Should().NotBeNullOrWhiteSpace();
            response.Location.AbsoluteUri.Should().StartWith(TestConfig.OrderBaseUri.ToString());
        }

        [Fact]
        public void it_cancel_reservation()
        {
            var orderApiClient = new OrderApiClient(Int32.Parse(TestConfig.MerchantId), TestConfig.SharedSecret, TestConfig.Locale, false);

            var cancelResult = orderApiClient.CancelReservation("1226560000");
    
            cancelResult.ShouldBeEquivalentTo(true);
        }

        [Fact]
        public void it_updates_orderid()
        {
            var client = new CheckoutClient(TestConfig.OrderBaseUri, TestConfig.MerchantId, TestConfig.SharedSecret);

            var result = client.UpdateOrderId("FZMN086AK49DDYA4YTO0JJPQKBG", "BB-ZNO-1516");

            result.ShouldBeEquivalentTo(true);
        }
    }
}
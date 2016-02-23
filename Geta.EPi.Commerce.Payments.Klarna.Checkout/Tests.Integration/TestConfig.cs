using System;
using Geta.Klarna.Checkout.Models;

namespace Tests.Integration
{
    public static class TestConfig
    {
        public static readonly Uri OrderBaseUri = new Uri("https://checkout.testdrive.klarna.com/checkout/orders");
        public const string MerchantId = "5982";
        public const string SharedSecret = "ueyxX5mwqSngE8e";
        public static Locale Locale = Locale.Norway;
    }
}
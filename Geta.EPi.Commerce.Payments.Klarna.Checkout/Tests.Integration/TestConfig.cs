using System;
using Geta.Klarna.Checkout.Models;

namespace Tests.Integration
{
    public static class TestConfig
    {
        public static readonly Uri OrderBaseUri = new Uri("https://checkout.testdrive.klarna.com/checkout/orders");
        public const string MerchantId = "";
        public const string SharedSecret = "";
        public static Locale Locale = Locale.Norway;
    }
}
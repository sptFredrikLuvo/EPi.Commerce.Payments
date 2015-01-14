using System;

namespace Tests.Integration
{
    public static class TestConfig
    {
        public static readonly Uri OrderBaseUri = new Uri("https://checkout.testdrive.klarna.com/checkout/orders");
        public const string MerchantId = "xxx";
        public const string SharedSecret = "xxx";
    }
}
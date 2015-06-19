using System;

namespace Tests.Integration
{
    public static class TestConfig
    {
        public static readonly Uri OrderBaseUri = new Uri("https://checkout.testdrive.klarna.com/checkout/orders");
        public const string MerchantId = "2943";
        public const string SharedSecret = "mnvCDwuWjiLG2no";
    }
}
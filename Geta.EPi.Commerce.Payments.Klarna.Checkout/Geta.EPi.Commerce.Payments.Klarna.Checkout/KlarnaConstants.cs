namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    public static class KlarnaConstants
    {
        public static readonly string Url = "Url";
        public static readonly string MerchantId = "MerchantId";
        public static readonly string Secret = "Secret";
        public static readonly string IsProduction = "IsProduction";
        public static readonly string NewsletterDefaultChecked = "NewsletterDefaultChecked";    
        public static readonly string Locale = "Locale";
        public const string ProductionBaseUri = "https://checkout.klarna.com/checkout/orders";
        public const string TestBaseUri = "https://checkout.testdrive.klarna.com/checkout/orders";
    }
}
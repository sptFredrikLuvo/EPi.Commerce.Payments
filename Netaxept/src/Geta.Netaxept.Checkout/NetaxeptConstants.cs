namespace Geta.Netaxept.Checkout
{
    public static class NetaxeptConstants
    {
        public const string NetaxeptPaymentMethod = "NetaxeptPaymentMethod";
        public const string SuccessfullUrl = "SuccessfullUrl";
        //public const string TransactionId = "TransactionId";

        public const string OrderNamespace = "Mediachase.Commerce.Orders";
        public const string OtherPaymentClass = "OtherPayment";

        /* Payment method fields*/
        public const string MerchantIdField = "MerchantId";
        public const string TokenField = "Token";
        public const string TerminalUrlField = "TerminalUrl";
        public const string TerminalLanguageField = "TerminalLanguage";

        public const string PaymentResultCookieName = "NetaxeptPaymentResult";

        public const string CustomerPanHashFieldName = "PanHash";
    }
}

namespace Geta.Netaxept.Checkout
{
    public static class NetaxeptConstants
    {
        /* Payment fields */
        public const string NetaxeptPaymentMethod = "NetaxeptPaymentMethod";
        public const string SuccessfullUrl = "SuccessfullUrl";
        public const string CardInformationMaskedPanField = "CardInformationMaskedPan";
        public const string CardInformationIssuerField = "CardInformationIssuer";
        public const string CardInformationExpiryDateField = "CardInformationExpiryDate";
        public const string CardInformationIssuerCountryField = "CardInformationIssuerCountry";
        public const string CardInformationIssuerIdField = "CardInformationIssuerId";
        public const string CardInformationPaymentMethodField = "CardInformationPaymentMethod";

        public const string OrderNamespace = "Mediachase.Commerce.Orders";
        public const string OtherPaymentClass = "OtherPayment";
        public const string CartClass = "ShoppingCart";

        /* Cart fields */
        public const string CartOrderNumberTempField = "CartOrderNumberTemp";

        /* Payment method fields*/
        public const string MerchantIdField = "MerchantId";
        public const string TokenField = "Token";
        public const string TerminalUrlField = "TerminalUrl";
        public const string TerminalLanguageField = "TerminalLanguage";

        public const string PaymentResultCookieName = "NetaxeptPaymentResult";

        public const string CustomerPanHashFieldName = "PanHash";
    }
}

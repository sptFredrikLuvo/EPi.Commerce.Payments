namespace Geta.Netaxept.Checkout
{
    public static class NetaxeptConstants
    {
        public const string PaymentResultCookieName = "NetaxeptPaymentResult";

        /* Payment fields */
        public const string NetaxeptPaymentMethod = "NetaxeptPaymentMethod";
        public const string CallbackUrl = "CallbackUrl";
        public const string CardInformationMaskedPanField = "CardInformationMaskedPan";
        public const string CardInformationIssuerField = "CardInformationIssuer";
        public const string CardInformationExpiryDateField = "CardInformationExpiryDate";
        public const string CardInformationIssuerCountryField = "CardInformationIssuerCountry";
        public const string CardInformationIssuerIdField = "CardInformationIssuerId";
        public const string CardInformationPaymentMethodField = "CardInformationPaymentMethod";

        /* Commerce fields */
        public const string OrderNamespace = "Mediachase.Commerce.Orders";
        public const string OtherPaymentClass = "OtherPayment";
        public const string CartClass = "ShoppingCart";

        /* Cart fields */
        public const string CartOrderNumberTempField = "CartOrderNumberTemp";

        /* Payment method fields*/
        public const string MerchantIdField = "MerchantId";
        public const string TokenField = "Token";
        public const string IsProductionField = "IsProduction";
        public const string TerminalLanguageField = "TerminalLanguage";

        /* Netaxept fields */
        public const string TerminalTestUrl = "https://test.epayment.nets.eu/terminal/default.aspx";
        public const string TerminalProductionUrl = "https://epayment.nets.eu/terminal/default.aspx";
        public const string NetaxeptServiceTestAddress = "https://test.epayment.nets.eu/netaxept.svc";
        public const string NetaxeptServiceProductionAddress = "https://epayment.nets.eu/netaxept.svc";

        /* Customer fields */
        public const string CustomerPanHashFieldName = "PanHash";
    }
}

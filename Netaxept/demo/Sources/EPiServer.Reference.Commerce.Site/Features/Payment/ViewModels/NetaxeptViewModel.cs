using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class NetaxeptViewModel : PaymentMethodViewModel<NetaxeptPaymentMethod>
    {
        public string CustomerCardMaskedFieldName { get; set; }
        public string CustomerCardExpirationDateFieldName { get; set; }
        public string CustomerCardPaymentMethodField { get; set; }
        public string CustomerCardIssuerCountryField { get; set; }
        public string CustomerCardIssuerIdField { get; set; }
        public string CustomerCardIssuerField { get; set; }
    }
}
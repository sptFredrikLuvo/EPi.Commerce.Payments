using System.Web.Mvc;
using Geta.Commerce.Payments.Verifone.HostedPages.Mvc;
using Geta.Verifone;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Models
{
    [ModelBinder(typeof(VerifoneModelBinder))]
    public class PaymentSuccessResponse
    {
        [BindAlias(VerifoneConstants.ParameterName.TransactionNumber)]
        public string TransactionNumber { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.PaymentMethodCodeResponse)]
        public string PaymentMethodCode { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.OrderNumber)]
        public string OrderNumber { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.OrderNote)]
        public string OrderNote { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.OrderTimestamp)]
        public string OrderTimestamp { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.OrderCurrencyCode)]
        public string OrderCurrencyCode { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.OrderGrossAmount)]
        public string OrderGrossAmount { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.SoftwareVersion)]
        public string SoftwareVersion { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.InterfaceVersion)]
        public string InterfaceVersion { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.ReferenceNumber)]
        public string ReferenceNumber { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.SignatureOne)]
        public string SignatureOne { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.SignatureTwo)]
        public string SignatureTwo { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.Token)]
        public string Token { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.FilingCode)]
        public string FilingCode { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.SocialSecurityNumber)]
        public string SocialSecurityNumber { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.BuyerEmailAddress)]
        public string BuyerEmailAddress { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.BuyerPhoneNumber)]
        public string BuyerPhoneNumber { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressCity)]
        public string DeliveryAddressCity { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressLineOne)]
        public string DeliveryAddressLineOne { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressLineTwo)]
        public string DeliveryAddressLineTwo { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressPostalCode)]
        public string DeliveryAddressPostalCode { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressCountryCode)]
        public string DeliveryAddressCountryCode { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.CardExpectedValidity)]
        public string CardExpectedValidity { get; set; }
    }
}
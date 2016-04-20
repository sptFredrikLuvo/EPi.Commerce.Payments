using System;
using System.Globalization;
using System.Linq;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Geta.Commerce.Payments.Verifone.HostedPages.Models;
using Geta.Verifone;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Extensions
{
    public static class VerifoneCreditCardPaymentExtensions
    {
        public static void InitializeFromOrderGroup(this VerifoneCreditCardPayment request, OrderGroup orderGroup)
        {
            var currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
            var paymentService = ServiceLocator.Current.GetInstance<IVerifonePaymentService>();
            OrderAddress orderAddress = orderGroup.OrderAddresses.First();

            request.MerchantAgreementCode = GetMerchantAgreementCode(request);
            request.PaymentLocale = paymentService.GetPaymentLocale(ContentLanguage.PreferredCulture);
            request.OrderNumber = orderGroup.OrderGroupId.ToString(CultureInfo.InvariantCulture.NumberFormat);
            request.OrderCurrencyCode = Iso4217Lookup.LookupByCode(currentMarket.GetCurrentMarket().DefaultCurrency.CurrencyCode).Number.ToString();
            request.OrderGrossAmount = orderGroup.Total.ToAmountString();
            request.OrderNetAmount = (orderGroup.Total - orderGroup.TaxTotal).ToAmountString();
            request.OrderVatAmount = orderGroup.TaxTotal.ToAmountString();
            request.BuyerFirstName = orderAddress.FirstName;
            request.BuyerLastName = orderAddress.LastName;

            string phoneNumber = orderAddress.DaytimePhoneNumber ?? orderAddress.EveningPhoneNumber;

            if (string.IsNullOrWhiteSpace(phoneNumber) == false)
                request.BuyerPhoneNumber = phoneNumber;

            request.BuyerEmailAddress = orderAddress.Email;
            request.DeliveryAddressLineOne = orderAddress.Line1;

            if (string.IsNullOrWhiteSpace(orderAddress.Line2) == false)
                request.DeliveryAddressLineTwo = orderAddress.Line2;

            request.DeliveryAddressPostalCode = orderAddress.PostalCode;
            request.DeliveryAddressCity = orderAddress.City;
            request.DeliveryAddressCountryCode = "578";
        }

        public static void ApplyPaymentMethodConfiguration(this VerifoneCreditCardPayment payment)
        {
            payment.InterfaceVersion = "3";
            payment.ShortCancelUrl = payment.GetParameterValue(VerifoneConstants.Configuration.CancelUrl, "/error").ToExternalUrl();
            payment.ShortErrorUrl = payment.GetParameterValue(VerifoneConstants.Configuration.ErrorUrl, "/error").ToExternalUrl();
            payment.ShortExpiredUrl = payment.GetParameterValue(VerifoneConstants.Configuration.ExpiredUrl, "/error").ToExternalUrl();
            payment.ShortRejectedUrl = payment.GetParameterValue(VerifoneConstants.Configuration.RejectedUrl, "/error").ToExternalUrl();
            payment.ShortSuccessUrl = payment.GetParameterValue(VerifoneConstants.Configuration.SuccessUrl, "/success").ToExternalUrl();
            payment.Software = payment.GetParameterValue(VerifoneConstants.Configuration.WebShopName, "My web shop");
            payment.SoftwareVersion = "1.0.0";
            payment.Submit = "Submit";
        }

        public static string GetPaymentUrl(this VerifoneCreditCardPayment payment)
        {
            PaymentMethodDto paymentMethodDto = GetPaymentMethodDto(payment);

            return paymentMethodDto != null
                ? paymentMethodDto.GetPaymentUrl()
                : null;
        }

        public static string GetMerchantAgreementCode(this VerifoneCreditCardPayment payment)
        {
            var paymentMethodDto = GetPaymentMethodDto(payment);

            return paymentMethodDto != null
                ? paymentMethodDto.GetMerchantAgreementCode()
                : "demo-agreement-code";
        }

        public static string GetParameterValue(this VerifoneCreditCardPayment payment, string parameterName, string defaultValue = null)
        {
            PaymentMethodDto paymentMethodDto = GetPaymentMethodDto(payment);

            return paymentMethodDto != null
                ? paymentMethodDto.GetParameter(parameterName, defaultValue)
                : defaultValue;
        }

        internal static PaymentMethodDto GetPaymentMethodDto(this VerifoneCreditCardPayment payment)
        {
            if (payment.PaymentMethodId != Guid.Empty)
            {
                PaymentMethodDto paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
                return paymentMethodDto;
            }

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Geta.Commerce.Payments.Verifone.HostedPages.Extensions;
using Geta.Commerce.Payments.Verifone.HostedPages.Models;
using Geta.Verifone;
using Geta.Verifone.Extensions;
using Geta.Verifone.Security;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Commerce.Payments.Verifone.HostedPages
{
    [ServiceConfiguration(typeof(IVerifonePaymentService))]
    public class DefaultVerifonePaymentService : IVerifonePaymentService
    {
        protected readonly IMarket CurrentMarket;
        protected readonly OrderContext OrderContext;

        public DefaultVerifonePaymentService(ICurrentMarket currentMarket)
        {
            if (currentMarket == null) throw new ArgumentNullException("currentMarket");
            CurrentMarket = currentMarket.GetCurrentMarket();
            OrderContext = OrderContext.Current;
        }

        #region Public Methods

        public virtual string GetPaymentLocale(CultureInfo culture)
        {
            switch (culture.Name)
            {
                case "no":
                case "nb":
                case "nn":
                case "nb-NO":
                case "nn-NO":
                    return "no_NO";
                case "sv":
                case "sv-SE":
                    return "sv_SE";
                case "sv-FI":
                    return "sv_FI";
                case "fi-FI":
                case "fi":
                    return "fi_FI";
                case "da-DK":
                case "da":
                    return "dk_DK";
                default:
                    return "en_GB";
            }
        }

        public virtual string GetPaymentUrl(VerifonePaymentRequest payment)
        {
            PaymentMethodDto paymentMethodDto = GetPaymentMethodDto(payment);

            return paymentMethodDto != null
                ? paymentMethodDto.GetPaymentUrl()
                : null;
        }

        /// <summary>
        /// Initialize a payment request from an <see cref="OrderGroup"/> instance. Adds order number, amount, timestamp, buyer information etc.
        /// </summary>
        /// <param name="payment">The <see cref="VerifonePaymentRequest"/> instance to initialize</param>
        /// <param name="orderGroup"><see cref="OrderGroup"/></param>
        public virtual void InitializePaymentRequest(VerifonePaymentRequest payment, OrderGroup orderGroup)
        {
            OrderAddress orderAddress = orderGroup.OrderAddresses.First();

            payment.OrderTimestamp = orderGroup.Created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            payment.MerchantAgreementCode = this.GetMerchantAgreementCode(payment);
            payment.PaymentLocale = this.GetPaymentLocale(ContentLanguage.PreferredCulture);
            payment.OrderNumber = orderGroup.OrderGroupId.ToString(CultureInfo.InvariantCulture.NumberFormat);

            payment.OrderCurrencyCode = this.IsProduction(payment) 
                ? Iso4217Lookup.LookupByCode(CurrentMarket.DefaultCurrency.CurrencyCode).Number.ToString()
                : "978";

            payment.OrderGrossAmount = orderGroup.Total.ToVerifoneAmountString();
            payment.OrderNetAmount = (orderGroup.Total - orderGroup.TaxTotal).ToVerifoneAmountString();
            payment.OrderVatAmount = orderGroup.TaxTotal.ToVerifoneAmountString();
            payment.BuyerFirstName = orderAddress.FirstName;
            payment.BuyerLastName = orderAddress.LastName;
            payment.OrderVatPercentage = "0";
            payment.PaymentMethodCode = "";
            payment.SavedPaymentMethodId = "";
            payment.StyleCode = "";
            payment.RecurringPayment = "0";
            payment.DeferredPayment = "0";
            payment.SavePaymentMethod = "0";
            payment.SkipConfirmationPage = "0";

            string phoneNumber = orderAddress.DaytimePhoneNumber ?? orderAddress.EveningPhoneNumber;

            if (string.IsNullOrWhiteSpace(phoneNumber) == false)
                payment.BuyerPhoneNumber = phoneNumber;

            payment.BuyerEmailAddress = orderAddress.Email;
            payment.DeliveryAddressLineOne = orderAddress.Line1;

            if (string.IsNullOrWhiteSpace(orderAddress.Line2) == false)
                payment.DeliveryAddressLineTwo = orderAddress.Line2;

            payment.DeliveryAddressPostalCode = orderAddress.PostalCode;
            payment.DeliveryAddressCity = orderAddress.City;
            payment.DeliveryAddressCountryCode = "246";

            ApplyPaymentMethodConfiguration(payment);
        }

        /// <summary>
        /// Validates a successful payment against the order.
        /// </summary>
        /// <param name="response">The success response posted from Verifone <see cref="PaymentSuccessResponse"/></param>
        /// <returns><see cref="StatusCode"/></returns>
        public virtual StatusCode ValidateSuccessReponse(PaymentSuccessResponse response)
        {
            OrderGroup order = this.OrderContext.GetCart(int.Parse(response.OrderNumber));

            if (order == null)
            {
                return StatusCode.OrderNotFound;
            }

            if (string.IsNullOrWhiteSpace(response.TransactionNumber))
            {
                return StatusCode.TransactionNumberMissing;
            }

            if (string.IsNullOrWhiteSpace(response.InterfaceVersion))
            {
                return StatusCode.InterfaceVersionMissing;
            }

            if (string.IsNullOrWhiteSpace(response.OrderCurrencyCode))
            {
                return StatusCode.OrderCurrencyCodeMissing;
            }

            if (string.IsNullOrWhiteSpace(response.OrderGrossAmount))
            {
                return StatusCode.OrderGrossAmountMissing;
            }

            if (string.IsNullOrWhiteSpace(response.SoftwareVersion))
            {
                return StatusCode.SoftwareVersionMissing;
            }

            if (string.IsNullOrWhiteSpace(response.OrderTimestamp))
            {
                return StatusCode.OrderTimestampMissing;
            }

            if (response.OrderTimestamp.Equals(order.Created.ToVerifoneDateString(), StringComparison.InvariantCulture) == false)
            {
                return StatusCode.OrderTimestampMismatch;
            }

            if (response.OrderGrossAmount.Equals(order.Total.ToVerifoneAmountString()) == false)
            {
                return StatusCode.OrderGrossAmountMismatch;
            }

            // TODO: Find a way to verify this both for test and production. Not used at the moment to simplify testing but needs to be implemented before release.

            //if (response.OrderCurrencyCode.Equals(Iso4217Lookup.LookupByCode(order.BillingCurrency).Number.ToString(CultureInfo.InvariantCulture)) == false)
            //{
            //    return StatusCode.OrderCurrencyCodeMismatch;
            //}

            if (this.VerifySignatures(response) == false)
            {
                return StatusCode.SignatureInvalid;
            }

            return StatusCode.OK;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Applies payment method parameters based on an initialized <see cref="VerifonePaymentRequest"/> with a valid PaymentMethodId property value.
        /// </summary>
        /// <param name="payment"><see cref="VerifonePaymentRequest"/> instance.</param>
        protected virtual void ApplyPaymentMethodConfiguration(VerifonePaymentRequest payment)
        {
            payment.InterfaceVersion = "3";
            payment.ShortCancelUrl = GetConfigurationValue(payment, VerifoneConstants.Configuration.CancelUrl, "/error").ToExternalUrl();
            payment.ShortErrorUrl = GetConfigurationValue(payment, VerifoneConstants.Configuration.ErrorUrl, "/error").ToExternalUrl();
            payment.ShortExpiredUrl = GetConfigurationValue(payment, VerifoneConstants.Configuration.ExpiredUrl, "/error").ToExternalUrl();
            payment.ShortRejectedUrl = GetConfigurationValue(payment, VerifoneConstants.Configuration.RejectedUrl, "/error").ToExternalUrl();
            payment.ShortSuccessUrl = GetConfigurationValue(payment, VerifoneConstants.Configuration.SuccessUrl, "/success").ToExternalUrl();
            payment.Software = GetConfigurationValue(payment, VerifoneConstants.Configuration.WebShopName, "My web shop");
            payment.SoftwareVersion = "1.0.0";
        }

        protected virtual string GetMerchantAgreementCode(VerifonePaymentRequest payment)
        {
            var paymentMethodDto = GetPaymentMethodDto(payment);

            return paymentMethodDto != null
                ? paymentMethodDto.GetMerchantAgreementCode()
                : "demo-agreement-code";
        }

        protected virtual string GetConfigurationValue(VerifonePaymentRequest payment, string parameterName, string defaultValue = null)
        {
            PaymentMethodDto paymentMethodDto = GetPaymentMethodDto(payment);

            return paymentMethodDto != null
                ? paymentMethodDto.GetParameter(parameterName, defaultValue)
                : defaultValue;
        }

        protected virtual bool IsProduction(VerifonePaymentRequest payment)
        {
            PaymentMethodDto paymentMethodDto = GetPaymentMethodDto(payment);

            return paymentMethodDto.GetParameter(VerifoneConstants.Configuration.IsProduction, "0") == "1";
        }

        protected virtual PaymentMethodDto GetPaymentMethodDto(VerifonePaymentRequest payment)
        {
            if (payment.PaymentMethodId != Guid.Empty)
            {
                PaymentMethodDto paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
                return paymentMethodDto;
            }

            return null;
        }

        protected virtual bool VerifySignatures(PaymentSuccessResponse response)
        {
            SortedDictionary<string, string> parameters = response.GetParameters();

            string signatureOne = response.GetParameterValue(VerifoneConstants.ParameterName.SignatureOne);
            string signatureTwo = response.GetParameterValue(VerifoneConstants.ParameterName.SignatureTwo);

            if (string.IsNullOrWhiteSpace(signatureOne) || string.IsNullOrWhiteSpace(signatureTwo))
            {
                return false;
            }

            SortedDictionary<string, string> parametersCopy = new SortedDictionary<string, string>(parameters);

            parametersCopy.Remove(VerifoneConstants.ParameterName.SignatureOne);
            parametersCopy.Remove(VerifoneConstants.ParameterName.SignatureTwo);

            string content = PointSignatureUtil.FormatParameters(parametersCopy);
            X509Certificate2 certificate = PointCertificateUtil.GetPointCertificate();

            return PointSignatureUtil.VerifySignature(certificate, signatureOne, content, HashAlgorithm.SHA1)
                    && PointSignatureUtil.VerifySignature(certificate, signatureTwo, content, HashAlgorithm.SHA512);
        }

        #endregion
    }
}
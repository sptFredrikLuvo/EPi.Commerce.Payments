using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Geta.Commerce.Payments.Verifone.HostedPages.Mvc;
using Geta.Verifone;
using Geta.Verifone.Security;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Models
{
    [ModelBinder(typeof(VerifoneModelBinder))]
    public class PaymentInitializationRequest
    {
        private readonly SortedDictionary<string, string> _parameters;

        [BindAlias(VerifoneConstants.ParameterName.PaymentLocale)]
        public string PaymentLocale
        {
            get { return _parameters[VerifoneConstants.ParameterName.PaymentLocale]; }
            set { _parameters[VerifoneConstants.ParameterName.PaymentLocale] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.PaymentTimestamp)]
        public DateTime PaymentTimestamp
        {
            get
            {
                string value = _parameters[VerifoneConstants.ParameterName.PaymentTimestamp];

                if (string.IsNullOrWhiteSpace(value))
                {
                    return DateTime.UtcNow;
                }

                return DateTime.Parse(value);
            }
            set
            {
                _parameters[VerifoneConstants.ParameterName.PaymentTimestamp] = value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [BindAlias(VerifoneConstants.ParameterName.MerchantAgreementCode)]
        public string MerchantAgreementCode
        {
            get { return _parameters[VerifoneConstants.ParameterName.MerchantAgreementCode]; }
            set { _parameters[VerifoneConstants.ParameterName.MerchantAgreementCode] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.OrderNumber)]
        public string OrderNumber
        {
            get { return _parameters[VerifoneConstants.ParameterName.OrderNumber]; }
            set { _parameters[VerifoneConstants.ParameterName.OrderNumber] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.OrderTimestamp)]
        public DateTime OrderTimestamp
        {
            get
            {
                string value = _parameters[VerifoneConstants.ParameterName.OrderTimestamp];

                if (string.IsNullOrWhiteSpace(value))
                {
                    return DateTime.UtcNow;
                }

                return DateTime.Parse(value);
            }
            set
            {
                _parameters[VerifoneConstants.ParameterName.OrderTimestamp] = value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [BindAlias(VerifoneConstants.ParameterName.OrderCurrencyCode)]
        public string OrderCurrencyCode
        {
            get { return _parameters[VerifoneConstants.ParameterName.OrderCurrencyCode]; }
            set { _parameters[VerifoneConstants.ParameterName.OrderCurrencyCode] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.OrderNetAmount)]
        public string OrderNetAmount
        {
            get { return _parameters[VerifoneConstants.ParameterName.OrderNetAmount]; }
            set { _parameters[VerifoneConstants.ParameterName.OrderNetAmount] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.OrderGrossAmount)]
        public string OrderGrossAmount
        {
            get { return _parameters[VerifoneConstants.ParameterName.OrderGrossAmount]; }
            set { _parameters[VerifoneConstants.ParameterName.OrderGrossAmount] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.OrderVatAmount)]
        public string OrderVatAmount
        {
            get { return _parameters[VerifoneConstants.ParameterName.OrderVatAmount]; }
            set { _parameters[VerifoneConstants.ParameterName.OrderVatAmount] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.OrderVatPercentage)]
        public string OrderVatPercentage
        {
            get { return _parameters[VerifoneConstants.ParameterName.OrderVatPercentage]; }
            set { _parameters[VerifoneConstants.ParameterName.OrderVatPercentage] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.BuyerFirstName)]
        public string BuyerFirstName
        {
            get { return _parameters[VerifoneConstants.ParameterName.BuyerFirstName]; }
            set { _parameters[VerifoneConstants.ParameterName.BuyerFirstName] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.BuyerLastName)]
        public string BuyerLastName
        {
            get { return _parameters[VerifoneConstants.ParameterName.BuyerLastName]; }
            set { _parameters[VerifoneConstants.ParameterName.BuyerLastName] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.BuyerPhoneNumber)]
        public string BuyerPhoneNumber
        {
            get { return _parameters[VerifoneConstants.ParameterName.BuyerPhoneNumber]; }
            set { _parameters[VerifoneConstants.ParameterName.BuyerPhoneNumber] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.BuyerEmailAddress)]
        public string BuyerEmailAddress
        {
            get { return _parameters[VerifoneConstants.ParameterName.BuyerEmailAddress]; }
            set { _parameters[VerifoneConstants.ParameterName.BuyerEmailAddress] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressLineOne)]
        public string DeliveryAddressLineOne
        {
            get { return _parameters[VerifoneConstants.ParameterName.DeliveryAddressLineOne]; }
            set { _parameters[VerifoneConstants.ParameterName.DeliveryAddressLineOne] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressLineTwo)]
        public string DeliveryAddressLineTwo
        {
            get { return _parameters[VerifoneConstants.ParameterName.DeliveryAddressLineTwo]; }
            set { _parameters[VerifoneConstants.ParameterName.DeliveryAddressLineTwo] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressLineThree)]
        public string DeliveryAddressLineThree
        {
            get { return _parameters[VerifoneConstants.ParameterName.DeliveryAddressLineThree]; }
            set { _parameters[VerifoneConstants.ParameterName.DeliveryAddressLineThree] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressCity)]
        public string DeliveryAddressCity
        {
            get { return _parameters[VerifoneConstants.ParameterName.DeliveryAddressCity]; }
            set { _parameters[VerifoneConstants.ParameterName.DeliveryAddressCity] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.DeliveryAddressPostalCode)]
        public string DeliveryAddressPostalCode
        {
            get { return _parameters[VerifoneConstants.ParameterName.DeliveryAddressPostalCode]; }
            set { _parameters[VerifoneConstants.ParameterName.DeliveryAddressPostalCode] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.PaymentMethodCode)]
        public string PaymentMethodCode
        {
            get { return _parameters[VerifoneConstants.ParameterName.PaymentMethodCode]; }
            set { _parameters[VerifoneConstants.ParameterName.PaymentMethodCode] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.SavedPaymentMethodId)]
        public string SavedPaymentMethodId
        {
            get { return _parameters[VerifoneConstants.ParameterName.SavedPaymentMethodId]; }
            set { _parameters[VerifoneConstants.ParameterName.SavedPaymentMethodId] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.StyleCode)]
        public string StyleCode
        {
            get { return _parameters[VerifoneConstants.ParameterName.StyleCode]; }
            set { _parameters[VerifoneConstants.ParameterName.StyleCode] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.RecurringPayment)]
        public string RecurringPayment
        {
            get { return _parameters[VerifoneConstants.ParameterName.RecurringPayment]; }
            set { _parameters[VerifoneConstants.ParameterName.RecurringPayment] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.DeferredPayment)]
        public string DeferredPayment
        {
            get { return _parameters[VerifoneConstants.ParameterName.DeferredPayment]; }
            set { _parameters[VerifoneConstants.ParameterName.DeferredPayment] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.SavePaymentMethod)]
        public string SavePaymentMethod
        {
            get { return _parameters[VerifoneConstants.ParameterName.SavePaymentMethod]; }
            set { _parameters[VerifoneConstants.ParameterName.SavePaymentMethod] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.SkipConfirmationPage)]
        public string SkipConfirmationPage
        {
            get { return _parameters[VerifoneConstants.ParameterName.SkipConfirmationPage]; }
            set { _parameters[VerifoneConstants.ParameterName.SkipConfirmationPage] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.ShortSuccessUrl)]
        public string ShortSuccessUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.ShortSuccessUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.ShortSuccessUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.LongSuccessUrl)]
        public string LongSuccessUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.LongSuccessUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.LongSuccessUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.ShortRejectedUrl)]
        public string ShortRejectedUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.ShortRejectedUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.ShortRejectedUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.LongRejectedUrl)]
        public string LongRejectedUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.LongRejectedUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.LongRejectedUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.ShortCancelUrl)]
        public string ShortCancelUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.ShortCancelUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.ShortCancelUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.LongCancelUrl)]
        public string LongCancelUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.LongCancelUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.LongCancelUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.ShortExpiredUrl)]
        public string ShortExpiredUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.ShortExpiredUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.ShortExpiredUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.LongExpiredUrl)]
        public string LongExpiredUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.LongExpiredUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.LongExpiredUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.ShortErrorUrl)]
        public string ShortErrorUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.ShortErrorUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.ShortErrorUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.LongErrorUrl)]
        public string LongErrorUrl
        {
            get { return _parameters[VerifoneConstants.ParameterName.LongErrorUrl]; }
            set { _parameters[VerifoneConstants.ParameterName.LongErrorUrl] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.Software)]
        public string Software
        {
            get { return _parameters[VerifoneConstants.ParameterName.Software]; }
            set { _parameters[VerifoneConstants.ParameterName.Software] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.SoftwareVersion)]
        public string SoftwareVersion
        {
            get { return _parameters[VerifoneConstants.ParameterName.SoftwareVersion]; }
            set { _parameters[VerifoneConstants.ParameterName.SoftwareVersion] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.InterfaceVersion)]
        public string InterfaceVersion
        {
            get { return _parameters[VerifoneConstants.ParameterName.InterfaceVersion]; }
            set { _parameters[VerifoneConstants.ParameterName.InterfaceVersion] = value; }
        }

        [BindAlias(VerifoneConstants.ParameterName.SignatureOne)]
        public string SignatureOne
        {
            get
            {
                string content = PointSignatureUtil.FormatParameters(_parameters);
                string signatureOne = PointSignatureUtil.CreateSignature(PointCertificateUtil.GetMerchantCertificate(), content, HashAlgorithm.SHA1);
                _parameters[VerifoneConstants.ParameterName.SignatureOne] = signatureOne;
                return signatureOne;
            }
        }

        [BindAlias(VerifoneConstants.ParameterName.SignatureTwo)]
        public string SignatureTwo
        {
            get { return _parameters[VerifoneConstants.ParameterName.SignatureTwo]; }
            set { _parameters[VerifoneConstants.ParameterName.SignatureTwo] = value; }
        }

        public readonly IList<BasketItem> BasketItems;

        public SortedDictionary<string, string> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public PaymentInitializationRequest()
        {
            _parameters = new SortedDictionary<string, string>();
            InitializeParameters();
            BasketItems = new List<BasketItem>();
        }

        private void InitializeParameters()
        {
            var utcNow = DateTime.UtcNow;
            PaymentLocale = "no_NO";
            PaymentTimestamp = utcNow;
            OrderTimestamp = utcNow;
        }

        public virtual SortedDictionary<string, string> GetParameters()
        {
            EnsureParameters();
            return _parameters;
        } 

        protected virtual void EnsureParameters()
        {
            _parameters[VerifoneConstants.ParameterName.PaymentToken] = CreatePaymentToken();
            //_parameters[VerifoneConstants.ParameterName.SignatureOne] = CreateSignatureOne();
        }

        protected virtual string CreatePaymentToken()
        {
            return PointSignatureUtil.CreatePaymentToken(_parameters);
        }

        protected virtual string CreateSignatureOne()
        {
            string content = PointSignatureUtil.FormatParameters(_parameters);
            return PointSignatureUtil.CreateSignature(PointCertificateUtil.GetMerchantCertificate(), content, HashAlgorithm.SHA1);
        }
    }
}
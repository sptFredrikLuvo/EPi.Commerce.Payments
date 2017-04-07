using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class NetaxeptPaymentMethod : PaymentMethodBase
    {
        public NetaxeptPaymentMethod()
            : this(LocalizationService.Current, ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        {
        }

        public NetaxeptPaymentMethod(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory)
            : base(localizationService, orderGroupFactory)
        {
        }

        public override bool ValidateData()
        {
            return true;
        }
        
        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = "netaxept";
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();

            var urlHelper = new UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);
            var netaxeptPaymentCallbackUrl = "http://" + System.Web.HttpContext.Current.Request.Url.DnsSafeHost +
                                                       urlHelper.Action("Index", "PaymentCallback");

            // creating the payment object
            payment.Properties[NetaxeptConstants.CallbackUrl] = netaxeptPaymentCallbackUrl;

            return payment;
        }

        public override void PostProcess(IPayment payment)
        {
            payment.Status = PaymentStatus.Processed.ToString();
        }
    }
}
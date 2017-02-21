using EPiServer.Commerce.Order;
using EPiServer.Commerce.Orders.Internal;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Geta.Commerce.Payments.PayPal.Extensions;
using Mediachase.Commerce.Orders;


namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class PayPalPaymentMethod : PaymentMethodBase
    {
        public PayPalPaymentMethod() : this(LocalizationService.Current, ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        {
        }

        public PayPalPaymentMethod(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory) : base(localizationService, orderGroupFactory)
        {
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = _orderGroupFactory.CreatePayPalPayment(orderGroup);
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = "PayPal";
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();
            return payment;
        }

        public override void PostProcess(IPayment payment)
        {
        }

        public override bool ValidateData()
        {
            return true;
        }
    }
}
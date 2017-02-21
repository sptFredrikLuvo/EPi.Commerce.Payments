using EPiServer.Commerce.Order;

namespace Geta.Commerce.Payments.PayPal.Extensions
{
    public static class IOrderGroupBuilderExtensions
    {
        public static IPayment CreatePayPalPayment(this IOrderGroupBuilder orderGroupBuilder)
        {
            return (IPayment)new PayPalPayment();
        }
    }
}
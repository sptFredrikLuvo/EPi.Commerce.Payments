using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    public class CartOrderNumber
    {
        public static string GenerateOrderNumber(Cart cart)
        {
            return CartOrderNumberHelper.GetOrderNumber(cart);
        }
    }
}

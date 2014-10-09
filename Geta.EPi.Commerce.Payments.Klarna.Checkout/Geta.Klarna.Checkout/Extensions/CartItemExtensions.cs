using System.Collections.Generic;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class CartItemExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this ICartItem cartItem)
        {
            return new Dictionary<string, object>
            {
                {"type", cartItem.Type},
                {"reference", cartItem.Reference},
                {"name", cartItem.Name},
                {"quantity", cartItem.Quantity},
                {"unit_price", cartItem.UnitPrice},
                {"discount_rate", cartItem.DiscountRate},
                {"tax_rate", cartItem.TaxRate}
            };
        } 
    }
}
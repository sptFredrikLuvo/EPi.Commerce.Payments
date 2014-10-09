using System.Collections.Generic;
using System.Linq;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class CartExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this Cart cart)
        {
            var items = cart.CartItems
                .Select(item => item.ToDictionary())
                .ToList();

            return new Dictionary<string, object> { { "items", items } };
        }
    }
}
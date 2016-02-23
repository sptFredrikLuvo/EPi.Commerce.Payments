using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Klarna.Checkout.Models;
using Klarna.Api;

namespace Geta.Klarna.Checkout
{
    public static class ApiExtensions
    {
        public static void UpdateCart(this Api api, List<ICartItem> cartItems)
        {
            foreach (var cartItem in cartItems)
            {
                var goodsFlags = GoodsFlags.IncVAT;
                if (cartItem.Reference == "SHIPPING")
                    goodsFlags = goodsFlags | GoodsFlags.Shipping;
                api.AddArticle(cartItem.Quantity, cartItem.Reference, cartItem.Name, cartItem.UnitPrice, cartItem.TaxRate, cartItem.DiscountRate, goodsFlags);
            }
        }

    }
}

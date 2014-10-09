using System;
using System.Collections.Generic;

namespace Geta.Klarna.Checkout
{
    public class Cart
    {
        public IEnumerable<ICartItem> CartItems { get; private set; }

        public Cart(
             IEnumerable<ICartItem> cartItems)
        {
            if (cartItems == null) throw new ArgumentNullException("cartItems");
            CartItems = cartItems;
        }
    }
}
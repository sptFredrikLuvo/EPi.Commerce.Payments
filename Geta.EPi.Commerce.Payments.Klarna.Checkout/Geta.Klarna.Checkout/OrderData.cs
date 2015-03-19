using System;

namespace Geta.Klarna.Checkout
{
    internal class OrderData
    {
        public Merchant Merchant { get; private set; }
        public Cart Cart { get; private set; }
        public Locale Locale { get; private set; }
        public Options Options { get; private set; }

        public OrderData(Merchant merchant, Cart cart, Locale locale, Options options)
        {
            if (merchant == null) throw new ArgumentNullException("merchant");
            if (cart == null) throw new ArgumentNullException("cart");
            if (locale == null) throw new ArgumentNullException("locale");
            if (options == null) throw new ArgumentNullException("options");
            Merchant = merchant;
            Cart = cart;
            Locale = locale;
            Options = options;
        }
    }
}
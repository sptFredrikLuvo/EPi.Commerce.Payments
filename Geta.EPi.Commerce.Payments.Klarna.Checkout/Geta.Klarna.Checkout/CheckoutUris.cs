using System;

namespace Geta.Klarna.Checkout
{
    public class CheckoutUris
    {
        public CheckoutUris(Uri checkout, Uri confirmation, Uri push, Uri terms)
        {
            if (checkout == null) throw new ArgumentNullException("checkout");
            if (confirmation == null) throw new ArgumentNullException("confirmation");
            if (push == null) throw new ArgumentNullException("push");
            if (terms == null) throw new ArgumentNullException("terms");
            Terms = terms;
            Push = push;
            Confirmation = confirmation;
            Checkout = checkout;
        }

        public Uri Checkout { get; private set; }
        public Uri Confirmation { get; private set; }
        public Uri Push { get; private set; }
        public Uri Terms { get; private set; }
    }
}
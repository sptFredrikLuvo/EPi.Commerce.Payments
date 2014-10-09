using System;

namespace Geta.Klarna.Checkout
{
    public class CheckoutResponse
    {
        public CheckoutResponse(Uri location, string snippet)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (snippet == null) throw new ArgumentNullException("snippet");
            Location = location;
            Snippet = snippet;
        }

        public Uri Location { get; private set; }
        public string Snippet { get; private set; }
    }
}
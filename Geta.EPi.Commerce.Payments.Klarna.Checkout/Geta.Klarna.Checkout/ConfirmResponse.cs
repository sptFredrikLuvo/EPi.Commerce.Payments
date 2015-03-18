using System;

namespace Geta.Klarna.Checkout
{
    public class ConfirmResponse
    {
        public ConfirmResponse(Uri location, string snippet, BillingAddress billingAddress, ShippingAddress shippingAddress)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (snippet == null) throw new ArgumentNullException("snippet");
            if (billingAddress == null) throw new ArgumentNullException("billingAddress");
            if (shippingAddress == null) throw new ArgumentNullException("shippingAddress");
            Location = location;
            Snippet = snippet;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
        }

        public Uri Location { get; private set; }
        public string Snippet { get; private set; }
        public BillingAddress BillingAddress { get; private set; }
        public ShippingAddress ShippingAddress { get; private set; }
    }
}
using System;

namespace Geta.Klarna.Checkout.Models
{
    public class Merchant
    {
        public string MerchantId { get; private set; }
        public string SharedSecret { get; private set; }
        public Uri CheckoutUri { get; private set; }
        public Uri ConfirmationUri { get; private set; }
        public Uri PushUri { get; private set; }
        public Uri TermsUri { get; private set; }
        public Uri ValidationUri { get; private set; }

        public Merchant(
            string merchantId, 
            string sharedSecret,
            Uri checkoutUri, 
            Uri confirmationUri, 
            Uri pushUri, 
            Uri termsUri,
            Uri validationUri)
        {
            if (merchantId == null) throw new ArgumentNullException("merchantId");
            if (sharedSecret == null) throw new ArgumentNullException("sharedSecret");
            if (checkoutUri == null) throw new ArgumentNullException("checkoutUri");
            if (confirmationUri == null) throw new ArgumentNullException("confirmationUri");
            if (pushUri == null) throw new ArgumentNullException("pushUri");
            if (termsUri == null) throw new ArgumentNullException("termsUri");
            MerchantId = merchantId;
            SharedSecret = sharedSecret;
            CheckoutUri = checkoutUri;
            ConfirmationUri = confirmationUri;
            PushUri = pushUri;
            TermsUri = termsUri;
            ValidationUri = validationUri;
        }
    }
}
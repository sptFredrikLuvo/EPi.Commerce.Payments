using System;

namespace Geta.Klarna.Checkout.Models
{
    public class CheckoutResponse : IResult
    {
        public CheckoutResponse(string id, Uri location, string snippet, string status)
        {
            if (location == null) throw new ArgumentNullException("location");
            if (snippet == null) throw new ArgumentNullException("snippet");
            Location = location;
            Snippet = snippet;
            TransactionId = id;
            Status = status;
        }

        public Uri Location { get; private set; }
        public string Snippet { get; private set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
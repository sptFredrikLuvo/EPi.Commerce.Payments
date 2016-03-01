using System;

namespace Geta.Klarna.Checkout.Models
{
    public class ConfirmResponse 
    {
        public ConfirmResponse(string orderId, string snippet, BillingAddress billingAddress, ShippingAddress shippingAddress, string status, string reservationNumber, int totalCost)
        {
            if (orderId == null) throw new ArgumentNullException("orderId");
            if (snippet == null) throw new ArgumentNullException("snippet");
            if (billingAddress == null) throw new ArgumentNullException("billingAddress");
            if (shippingAddress == null) throw new ArgumentNullException("shippingAddress");
            if (totalCost == null) throw new ArgumentNullException("totalCost");
            Snippet = snippet;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
            Status = status;
            ReservationNumber = reservationNumber;
            TotalCost = totalCost;
            OrderId = orderId;
        }

        public string Snippet { get; private set; }
        public BillingAddress BillingAddress { get; private set; }
        public ShippingAddress ShippingAddress { get; private set; }
        public string Status { get; private set; }
        public string ReservationNumber { get; private set; }
        public int TotalCost { get; set; }
        public string OrderId { get; private set; }
    }
}
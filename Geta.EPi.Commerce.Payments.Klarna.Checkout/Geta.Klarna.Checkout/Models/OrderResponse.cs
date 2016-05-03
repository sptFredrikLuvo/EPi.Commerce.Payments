namespace Geta.Klarna.Checkout.Models
{
    public class OrderResponse
    {
        public string Snippet { get; set; }
        public int TotalCost { get; set; }
        public string CustomerName { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public BillingAddress BillingAddress { get; set; }

        public OrderResponse(string snippet, int totalCost, string customerName, ShippingAddress shippingAddress, BillingAddress billingAddress)
        {
            Snippet = snippet;
            TotalCost = totalCost;
            CustomerName = customerName;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
        }
    }
}
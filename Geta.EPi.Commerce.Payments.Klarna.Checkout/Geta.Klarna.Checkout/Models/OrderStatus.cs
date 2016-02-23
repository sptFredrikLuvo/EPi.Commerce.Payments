namespace Geta.Klarna.Checkout.Models
{
    public static class OrderStatus
    {
        public static readonly string InComplete = "checkout_incomplete"; // intial status
        public static readonly string Complete = "checkout_complete";
        public static readonly string Created = "created";
    }
}

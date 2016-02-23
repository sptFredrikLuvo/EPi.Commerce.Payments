namespace Geta.Klarna.Checkout.Models
{
    public class Options
    {
        public Options(bool allowSeparateShippingAddress)
        {
            AllowSeparateShippingAddress = allowSeparateShippingAddress;
        }

        public bool AllowSeparateShippingAddress { get; private set; }

        public string ButtonColorCode { get; set; }
    }
}
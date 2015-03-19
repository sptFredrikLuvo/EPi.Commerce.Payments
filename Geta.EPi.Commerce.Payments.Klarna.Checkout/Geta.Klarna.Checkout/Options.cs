namespace Geta.Klarna.Checkout
{
    public class Options
    {
        public Options(bool allowSeparateShippingAddress)
        {
            AllowSeparateShippingAddress = allowSeparateShippingAddress;
        }

        public bool AllowSeparateShippingAddress { get; private set; }
    }
}
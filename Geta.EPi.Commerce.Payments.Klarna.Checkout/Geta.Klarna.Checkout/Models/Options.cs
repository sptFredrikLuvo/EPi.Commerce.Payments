namespace Geta.Klarna.Checkout.Models
{
    public class Options
    {
        public Options(bool allowSeparateShippingAddress, bool enableOrganizationCheckout)
        {
            AllowSeparateShippingAddress = allowSeparateShippingAddress;
            EnableOrganizationCheckout = enableOrganizationCheckout;
        }
        
        public bool AllowSeparateShippingAddress { get; private set; }
        public ColorOptions ColorOptions { get; set; }

        public bool EnableOrganizationCheckout { get; set; }
    }
}
namespace Geta.Klarna.Checkout.Models
{
    public interface ICartItem
    {
        string Type { get; }
        string Reference { get; }
        string Name { get; }
        int Quantity { get; }
        int UnitPrice { get; }
        int DiscountRate { get; }
        int TaxRate { get; }
    }
}
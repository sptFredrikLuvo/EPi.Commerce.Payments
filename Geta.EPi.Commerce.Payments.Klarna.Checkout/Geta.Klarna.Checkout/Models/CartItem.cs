using System;

namespace Geta.Klarna.Checkout.Models
{
    public class CartItem : ICartItem
    {
        public string Type
        {
            get { return "physical"; }
        }

        public string Reference { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public int UnitPrice { get; private set; }
        public int DiscountRate { get; private set; }
        public int TaxRate { get; private set; }

        public CartItem(
            string reference,
            string name,
            int quantity,
            int unitPrice,
            int discountRate,
            int taxRate)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            if (name == null) throw new ArgumentNullException("name");
            Reference = reference;
            Name = name;
            Quantity = quantity;
            UnitPrice = unitPrice;
            DiscountRate = discountRate;
            TaxRate = taxRate;
        }
    }
}
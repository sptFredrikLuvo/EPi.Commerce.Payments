using System;
using Geta.Klarna.Checkout.Models;

namespace Geta.Klarna.Checkout
{
    public class ShippingItem : ICartItem
    {
        public string Type
        {
            get { return "shipping_fee"; }
        }

        public string Reference
        {
            get { return "SHIPPING"; }
        }

        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public int UnitPrice { get; private set; }
        public int DiscountRate { get; private set; }
        public int TaxRate { get; private set; }

        public ShippingItem(
            string name, 
            int quantity, 
            int unitPrice, 
            int discountRate,
            int taxRate)
        {
            DiscountRate = discountRate;
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TaxRate = taxRate;
        }
    }
}
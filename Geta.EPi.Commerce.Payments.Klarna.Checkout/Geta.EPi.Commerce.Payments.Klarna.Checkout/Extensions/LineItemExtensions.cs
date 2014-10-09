using System;
using Geta.Klarna.Checkout;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions
{
    public static class LineItemExtensions
    {
        public static ICartItem ToCartItem(this LineItem lineItem, decimal taxRate = 0)
        {
            return new CartItem(
                lineItem.CatalogEntryId,
                lineItem.DisplayName,
                Convert.ToInt32(lineItem.Quantity),
                Convert.ToInt32(lineItem.PlacedPrice * 100),
                Convert.ToInt32(lineItem.LineItemDiscountAmount * 100),
                Convert.ToInt32(taxRate * 100));
        }
    }
}
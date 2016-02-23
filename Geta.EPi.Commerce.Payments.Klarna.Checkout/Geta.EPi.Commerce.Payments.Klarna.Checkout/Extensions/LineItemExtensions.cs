using System;
using Geta.Klarna.Checkout.Models;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions
{
    public static class LineItemExtensions
    {
        public static ICartItem ToCartItem(this LineItem lineItem, bool isOrderUpdate = false)
        {
            var discountPerItem = (lineItem.LineItemDiscountAmount + lineItem.OrderLevelDiscountAmount)/ lineItem.Quantity;
            var discountRate = discountPerItem * 100 / lineItem.ListPrice;

            var vatPercent = lineItem.GetDecimalValue(MetadataConstants.VatPercent, 0);

            // Klarna uses different price and vat formats for checkout and order update

            if (isOrderUpdate)
            {
                return new CartItem(
                lineItem.Code,
                lineItem.DisplayName,
                Convert.ToInt32(lineItem.Quantity),
                Convert.ToInt32(lineItem.ListPrice),
                Convert.ToInt32(discountRate),
                Convert.ToInt32(vatPercent));
            }

            return new CartItem(
                lineItem.Code,
                lineItem.DisplayName,
                Convert.ToInt32(lineItem.Quantity),
                Convert.ToInt32(lineItem.ListPrice * 100),
                Convert.ToInt32(discountRate * 100),
                Convert.ToInt32(vatPercent * 100));
        }

    }
}
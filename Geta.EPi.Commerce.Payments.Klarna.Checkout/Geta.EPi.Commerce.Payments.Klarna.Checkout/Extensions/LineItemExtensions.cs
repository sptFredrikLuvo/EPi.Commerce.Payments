using System;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using Geta.Klarna.Checkout.Models;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions
{
    public static class LineItemExtensions
    {
        public static ICartItem ToCartItem(this ILineItem lineItem, bool isOrderUpdate = false)
        {
            var discountPerItem = lineItem.PlacedPrice - (lineItem.TryGetDiscountValue(x => x.OrderAmount) + lineItem.TryGetDiscountValue(x => x.EntryAmount)) / lineItem.Quantity;
            var discountRate = discountPerItem * 100 / lineItem.PlacedPrice;

            decimal vatPercent = 0;
            decimal.TryParse((string)lineItem.Properties[MetadataConstants.VatPercent], out vatPercent);

            // Klarna uses different price and vat formats for checkout and order update

            if (isOrderUpdate)
            {
                return new CartItem(
                    lineItem.Code,
                    lineItem.DisplayName,
                    Convert.ToInt32(lineItem.Quantity),
                    Convert.ToInt32(lineItem.PlacedPrice),
                    Convert.ToInt32(discountRate),
                    Convert.ToInt32(vatPercent));
            }

            return new CartItem(
                lineItem.Code,
                lineItem.DisplayName,
                Convert.ToInt32(lineItem.Quantity),
                Convert.ToInt32(lineItem.PlacedPrice * 100),
                Convert.ToInt32(discountRate * 100),
                Convert.ToInt32(vatPercent * 100));
        }

    }
}
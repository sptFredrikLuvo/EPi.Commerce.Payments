using System;
using System.Collections.Generic;
using System.Linq;
using Geta.Klarna.Checkout;
using Geta.Klarna.Checkout.Models;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions
{
    public static class ShipmentExtensions
    {
        public static List<ICartItem> ToCartItems(this Shipment shipment, bool isOrderUpdate = false)
        {
            int multiplyFactor = isOrderUpdate ? 1 : 100;

            var shippingtax = Convert.ToInt32(shipment.ShippingTax * multiplyFactor);

            var shipmentLines = new List<ICartItem>
            {
                new ShippingItem(
                    shipment.ShippingMethodName, 1, Convert.ToInt32(shipment.ShippingSubTotal*multiplyFactor), 0, shippingtax)
            };

            // add separate line for discount if discount exists
            if (shipment.ShippingDiscountAmount > 0)
            {
                var name = GetShipmentDiscountName(shipment);
                shipmentLines.Add(new ShippingItem(name, 1, Convert.ToInt32(shipment.ShippingSubTotal * multiplyFactor * -1), 0, shippingtax));
            }

            return shipmentLines;
        }

        private static string GetShipmentDiscountName(Shipment shipment)
        {
            var discounts = shipment.Discounts;
            if (discounts != null && discounts.Count > 0)
                return discounts[0].DisplayMessage; // pick the first discount and use display message

            return string.Format("{0} Discount", shipment.ShippingMethodName);
        }

    }
}
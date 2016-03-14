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
        public static ICartItem ToCartItem(this Shipment shipment, bool isOrderUpdate = false)
        {
            int multiplyFactor = isOrderUpdate ? 1 : 100;

            var shippingtax = Convert.ToInt32(shipment.ShippingTax * multiplyFactor);

            return new ShippingItem(
                    shipment.ShippingMethodName, 1, Convert.ToInt32(shipment.ShipmentTotal * multiplyFactor), 0, shippingtax);
        }

    }
}
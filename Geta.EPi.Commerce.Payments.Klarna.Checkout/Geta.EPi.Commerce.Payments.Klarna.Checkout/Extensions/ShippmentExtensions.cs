using System;
using Geta.Klarna.Checkout;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions
{
    public static class ShippmentExtensions
    {
        public static ICartItem ToCartItem(this Shipment shipment)
        {
            return new ShippingItem(
                    shipment.ShippingMethodName, 1, Convert.ToInt32(shipment.ShipmentTotal * 100), 0, 0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Klarna.Checkout;
using Geta.Klarna.Checkout.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions
{
    public static class ShipmentExtensions
    {
        private static Injected<ITaxCalculator> _taxCalculator;
        private static Injected<ICurrentMarket> _currentMarket;

        public static List<ICartItem> ToCartItems(this IShipment shipment, PromotionInformation shippingPromotion, Currency currency, bool isOrderUpdate = false)
        {
            int multiplyFactor = isOrderUpdate ? 1 : 100;
            var tax = _taxCalculator.Service.GetShippingTaxTotal(shipment, _currentMarket.Service.GetCurrentMarket(), currency);
            var shippingtax = Convert.ToInt32(tax * multiplyFactor);

            var shipmentLines = new List<ICartItem>
            {
                new ShippingItem(
                    shipment.ShippingMethodName, 1, Convert.ToInt32(shipment.GetShippingCost(_currentMarket.Service.GetCurrentMarket(), currency)*multiplyFactor), 0, shippingtax)
            };

            var shippingDiscountAmount = shipment.GetShipmentDiscountPrice(currency);
            // add separate line for discount if discount exists
            if (shippingDiscountAmount.Amount > 0)
            {
                var name = GetShipmentDiscountName(shipment, shippingPromotion);
                shipmentLines.Add(new ShippingItem(name, 1, Convert.ToInt32(shippingDiscountAmount * multiplyFactor * -1), 0, shippingtax));
            }

            return shipmentLines;
        }

        private static string GetShipmentDiscountName(IShipment shipment, PromotionInformation shippingPromotion)
        {
            if (shippingPromotion != null)
            {
                return shippingPromotion.Name;
            }

            return $"{shipment.ShippingMethodName} Discount";
        }

    }
}
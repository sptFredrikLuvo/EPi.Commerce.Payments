using System;
using EPiServer.Commerce.Order;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    public class CartOrderNumberHelper
    {
        public static string GenerateOrderNumber(IOrderGroup orderGroup)
        {
            var orderNumberField = orderGroup.Properties[NetaxeptConstants.CartOrderNumberTempField];

            if (orderNumberField == null)
            {
                int num = (new Random()).Next(100, 999);
                string str = num.ToString();
                var orderNumber = string.Format("PO{0}{1}", orderGroup.OrderLink.OrderGroupId, str);

                orderGroup.Properties[NetaxeptConstants.CartOrderNumberTempField] = orderNumber;
                return orderNumber;
            }

            return orderNumberField.ToString();
        }

        public static string GetOrderNumber(OrderGroup orderGroup)
        {
            return orderGroup.GetString(NetaxeptConstants.CartOrderNumberTempField);
        }
    }
}

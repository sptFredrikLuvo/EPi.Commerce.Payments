using System;
using System.Web;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    public class CartOrderNumberHelper
    {
        internal static string _orderNumberCookieKey = "OrderNumber_Temp";

        public static string GenerateOrderNumber(OrderGroup orderGroup)
        {
            int num = (new Random()).Next(100, 999);
            string str = num.ToString();

            var orderNumber = string.Format("PO{0}{1}", orderGroup.OrderGroupId, str);
            
            orderGroup.SetMetaField(NetaxeptConstants.CartOrderNumberTempField, orderNumber, false);

            return orderNumber;
        }

        public static string GetOrderNumber(OrderGroup orderGroup)
        {
            return orderGroup.GetString(NetaxeptConstants.CartOrderNumberTempField);
        }
    }
}

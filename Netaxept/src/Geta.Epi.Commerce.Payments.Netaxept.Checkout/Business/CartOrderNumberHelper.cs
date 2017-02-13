using System;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    public class CartOrderNumberHelper
    {
        public static string GenerateOrderNumber(OrderGroup orderGroup)
        {
            var metaField = orderGroup[NetaxeptConstants.CartOrderNumberTempField];

            if (metaField == null)
            {

                int num = (new Random()).Next(100, 999);
                string str = num.ToString();

                var orderNumber = string.Format("PO{0}{1}", orderGroup.OrderGroupId, str);

                orderGroup.SetMetaField(NetaxeptConstants.CartOrderNumberTempField, orderNumber, false);

                return orderNumber;
            }
            else
            {
                return metaField.ToString();
            }
        }

        public static string GetOrderNumber(OrderGroup orderGroup)
        {
            return orderGroup.GetString(NetaxeptConstants.CartOrderNumberTempField);
        }
    }
}

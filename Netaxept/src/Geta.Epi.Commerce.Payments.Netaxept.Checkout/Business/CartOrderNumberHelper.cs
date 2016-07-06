using System;
using System.Web;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    internal class CartOrderNumberHelper
    {
        private static string _orderNumberCookieKey = "OrderNumber_Temp";

        public static string GenerateOrderNumber(OrderGroup orderGroup)
        {
            int num = (new Random()).Next(100, 999);
            string str = num.ToString();

            var orderNumber = string.Format("PO{0}{1}", orderGroup.OrderGroupId, str);
            
            var cookie = new HttpCookie(_orderNumberCookieKey, orderNumber)
            {
                Expires = DateTime.Now.Add(new TimeSpan(0, 1, 0, 0))
            };
            if (HttpContext.Current.Response.Cookies[_orderNumberCookieKey] == null)
            {
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else
            {
                HttpContext.Current.Response.Cookies.Set(cookie);
            }

            return orderNumber;
        }

        public static string GetOrderNumber()
        {
            return HttpContext.Current.Request.Cookies[_orderNumberCookieKey].Value;
        }
    }
}

using System;
using System.Web;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment step helper class
    /// </summary>
    public static class PaymentStepHelper
    {
        /// <summary>
        /// Save transaction temporary to cookie
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="keyName"></param>
        /// <param name="timeSpan"></param>
        public static void SaveTransactionToCookie(Object obj, string keyName, TimeSpan timeSpan)
        {
            string myObjectJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var cookie = new HttpCookie(keyName, myObjectJson)
            {
                Expires = DateTime.Now.Add(timeSpan)
            };
            if (HttpContext.Current.Response.Cookies[keyName] == null)
            {
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else
            {
                HttpContext.Current.Response.Cookies.Set(cookie);
            }
        }

        /// <summary>
        /// Get transaction from cookie
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static T GetTransactionFromCookie<T>(string keyName)
        {
            if (HttpContext.Current.Request.Cookies[keyName] != null)
            {
                var s = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.Cookies[keyName].Value);
                return !string.IsNullOrEmpty(s) ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s) : default(T);
            }
            return default(T);
        }

        /// <summary>
        /// Get amount as string value (without decimals)
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static string GetAmount(decimal amount)
        {
            return Math.Round(amount * 100).ToString();
        }
    }
}

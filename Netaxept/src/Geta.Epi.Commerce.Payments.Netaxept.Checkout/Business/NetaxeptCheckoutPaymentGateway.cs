
using System;
using System.Web;
using EPiServer.Logging;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Website;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    public class NetaxeptCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentOption
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(NetaxeptCheckoutPaymentGateway));

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            try
            {
                Logger.Debug("Netaxept checkout gateway. Processing Payment ....");

                var transactionIdResult = GetObjectFromCookie<string>(NetaxeptConstants.PaymentResultCookieName);
                if (string.IsNullOrEmpty(transactionIdResult))
                {
                    var netaxeptServiceClient = new NetaxeptServiceClient();

                    var transactionId = netaxeptServiceClient.Register("12002339", "9Jq(_4Q");

                    SaveObjectToCookie(transactionId, NetaxeptConstants.PaymentResultCookieName,
                        new TimeSpan(0, 1, 0, 0));

                    var url =
                        string.Format(
                            "https://test.epayment.nets.eu/epay/default.aspx?merchantId={0}&transactionId={1}",
                            "12002339", transactionId);
                    HttpContext.Current.Response.Redirect(url);

                    return false;
                }
                else
                {
                    SaveObjectToCookie(null, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));

                    return true;
                }
            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                message = exception.Message;
                throw;
            }
        }

        public bool ValidateData()
        {
            return true;
        }

        public Payment PreProcess(OrderForm orderForm)
        {
            if (orderForm == null) throw new ArgumentNullException(nameof(orderForm));

            var payment = new NetaxeptPayment()
            {
                PaymentMethodId = PaymentMethodId,
                PaymentMethodName = "NetaxeptCheckout",
                OrderFormId = orderForm.OrderFormId,
                OrderGroupId = orderForm.OrderGroupId,
                Amount = orderForm.Total,
                Status = PaymentStatus.Pending.ToString(),
                TransactionType = TransactionType.Authorization.ToString(),
            };

            return payment;
        }

        public bool PostProcess(OrderForm orderForm)
        {
            return true;
        }

        private void SaveObjectToCookie(Object obj, string keyName, TimeSpan timeSpan)
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

        private T GetObjectFromCookie<T>(string keyName)
        {
            if (HttpContext.Current.Request.Cookies[keyName] != null)
            {
                var s = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.Cookies[keyName].Value);
                return !string.IsNullOrEmpty(s) ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s) : default(T);
            }
            return default(T);
        }

        public Guid PaymentMethodId { get; set; }
        public string SuccessUrl { get; set; }
    }
}

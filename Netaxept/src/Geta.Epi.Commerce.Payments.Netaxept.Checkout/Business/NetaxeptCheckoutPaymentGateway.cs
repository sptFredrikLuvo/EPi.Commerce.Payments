using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using EPiServer.Logging;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Website;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    /// <summary>
    /// Netaxept payment gateway
    /// </summary>
    public class NetaxeptCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentOption
    {
        private PaymentMethodDto _payment;
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(NetaxeptCheckoutPaymentGateway));

        public Guid PaymentMethodId { get; set; }
        public string SuccessUrl { get; set; }
        public string CallBackUrlWhenFail { get; set; }

        /// <summary>
        /// Process payment method. 
        /// This method is called twice, the first time it will redirect the user to the terminal of Netaxept.
        /// Second time we will complete the payment.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            try
            {
                Logger.Debug("Netaxept checkout gateway. Processing Payment ....");

                _payment = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);

                var netaxeptServiceClient = new NetaxeptServiceClient();
                OrderForm orderForm = payment.Parent as OrderForm;

                var transactionIdResult = GetTransactionFromCookie<string>(NetaxeptConstants.PaymentResultCookieName);
                if (string.IsNullOrEmpty(transactionIdResult))
                {
                    var transactionId = netaxeptServiceClient.Register(CreatePaymentRequest(payment, orderForm));

                    SaveTransactionToCookie(transactionId, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));

                    var url = new UriBuilder(_payment.GetParameter(NetaxeptConstants.TerminalUrlField));
                    var nvc = new NameValueCollection
                    {
                        { "merchantId", _payment.GetParameter(NetaxeptConstants.MerchantIdField) },
                        { "transactionId", transactionId }
                    };

                    url.Query = string.Join("&", nvc.AllKeys.Select(key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));

                    HttpContext.Current.Response.Redirect(url.ToString());

                    return false;
                }
                else
                {
                    var paymentResult = netaxeptServiceClient.Query("12002339", "9Jq(_4Q", transactionIdResult);

                    if (paymentResult.Cancelled)
                    {
                        message = "The payment was cancelled by the user.";
                        SaveTransactionToCookie(null, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                        return false;
                    }
                    if (paymentResult.ErrorOccurred)
                    {
                        message = paymentResult.ErrorMessage;
                        SaveTransactionToCookie(null, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));
                        return false;
                    }

                    // Save the PanHash(if not empty) on the customer contact, so we can use EasyPayment for next payment
                    if (!string.IsNullOrEmpty(paymentResult.PanHash) && orderForm.Parent.CustomerId != Guid.Empty)
                    {
                        var customerContact = CustomerContext.Current.GetContactById(orderForm.Parent.CustomerId);
                        if (customerContact != null)
                        {
                            customerContact[NetaxeptConstants.CustomerPanHashFieldName] = paymentResult.PanHash;
                            customerContact.SaveChanges();
                        }
                    }
                    SaveTransactionToCookie(null, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));

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

        /// <summary>
        /// Pre process method. Set all fields for the payment
        /// </summary>
        /// <param name="orderForm"></param>
        /// <returns></returns>
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
                TransactionType = TransactionType.Authorization.ToString()
            };

            payment.SetMetaField(NetaxeptConstants.SuccessfullUrl, SuccessUrl, false);

            return payment;
        }

        /// <summary>
        /// Will always return true
        /// </summary>
        /// <param name="orderForm"></param>
        /// <returns></returns>
        public bool PostProcess(OrderForm orderForm)
        {
            return true;
        }

        /// <summary>
        /// Save transaction to cookie
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="keyName"></param>
        /// <param name="timeSpan"></param>
        private void SaveTransactionToCookie(Object obj, string keyName, TimeSpan timeSpan)
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
        private T GetTransactionFromCookie<T>(string keyName)
        {
            if (HttpContext.Current.Request.Cookies[keyName] != null)
            {
                var s = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.Cookies[keyName].Value);
                return !string.IsNullOrEmpty(s) ? Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s) : default(T);
            }
            return default(T);
        }

        /// <summary>
        /// Create payment request by OrderForm object
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="orderForm"></param>
        /// <returns></returns>
        private PaymentRequest CreatePaymentRequest(Payment payment, OrderForm orderForm)
        {
            var request = new PaymentRequest();
            request.EnableEasyPayments = true;

            if (orderForm.Parent.CustomerId != Guid.Empty)
            {
                var customerContact = CustomerContext.Current.GetContactById(orderForm.Parent.CustomerId);
                if (customerContact != null)
                {
                    request.PanHash = customerContact[NetaxeptConstants.CustomerPanHashFieldName]?.ToString();
                }
            }

            request.Amount = Math.Round(orderForm.Total * 100).ToString();
            request.CurrencyCode = orderForm.Parent.BillingCurrency;
            request.OrderDescription = "Netaxept order";
            request.OrderNumber = orderForm.Parent.OrderGroupId.ToString();

            request.MerchantId = _payment.GetParameter(NetaxeptConstants.MerchantIdField);
            request.Token = _payment.GetParameter(NetaxeptConstants.TokenField);
            request.Language = _payment.GetParameter(NetaxeptConstants.TerminalLanguageField);

            request.SuccessUrl = payment.GetStringValue(NetaxeptConstants.SuccessfullUrl, string.Empty);

            return request;
        }
    }

    
}

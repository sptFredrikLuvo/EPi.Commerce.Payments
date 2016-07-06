using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment should be registered at Netaxept
    /// </summary>
    public class RegisterPaymentStep : PaymentStep
    {
        public RegisterPaymentStep(Payment payment) : base(payment)
        { }

        /// <summary>
        /// Process register payment step
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(Payment payment, ref string message)
        {
            var paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
            var transactionIdResult = PaymentStepHelper.GetTransactionFromCookie<string>(NetaxeptConstants.PaymentResultCookieName);

            if (payment.TransactionType == "Authorization" && string.IsNullOrEmpty(transactionIdResult))
            {
                var orderForm = payment.Parent;
                var transactionId = this.Client.Register(CreatePaymentRequest(paymentMethodDto, payment, orderForm));

                //AddNote(orderForm, "Payment - Registered", "Payment - Amount is registered");

                PaymentStepHelper.SaveTransactionToCookie(transactionId, NetaxeptConstants.PaymentResultCookieName, new TimeSpan(0, 1, 0, 0));

                var url = new UriBuilder(paymentMethodDto.GetParameter(NetaxeptConstants.TerminalUrlField));
                var nvc = new NameValueCollection
                    {
                        { "merchantId", paymentMethodDto.GetParameter(NetaxeptConstants.MerchantIdField) },
                        { "transactionId", transactionId }
                    };

                url.Query = string.Join("&", nvc.AllKeys.Select(key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));

                HttpContext.Current.Response.Redirect(url.ToString());

                return false;
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, ref message);
            }
            return false;
        }

        /// <summary>
        /// Create payment request
        /// </summary>
        /// <param name="paymentMethodDto"></param>
        /// <param name="payment"></param>
        /// <param name="orderForm"></param>
        /// <returns></returns>
        private PaymentRequest CreatePaymentRequest(PaymentMethodDto paymentMethodDto, Payment payment, OrderForm orderForm)
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
            
            request.Amount = PaymentStepHelper.GetAmount(orderForm.Total);
            request.CurrencyCode = orderForm.Parent.BillingCurrency;
            request.OrderDescription = "Netaxept order";
            request.OrderNumber = CartOrderNumberHelper.GenerateOrderNumber(orderForm.Parent); //orderForm.Parent.OrderGroupId.ToString(); Generate order number
            
            request.Language = paymentMethodDto.GetParameter(NetaxeptConstants.TerminalLanguageField);

            request.SuccessUrl = payment.GetStringValue(NetaxeptConstants.SuccessfullUrl, string.Empty);

            request.CustomerNumber = (orderForm.Parent.CustomerId != Guid.Empty ? orderForm.Parent.CustomerId.ToString() : string.Empty);

            var billingAddress = orderForm.Parent.OrderAddresses.FirstOrDefault(a => a.Name == orderForm.BillingAddressId);
            if (billingAddress != null)
            {
                request.CustomerFirstname = billingAddress.FirstName;
                request.CustomerLastname = billingAddress.LastName;
                request.CustomerEmail = billingAddress.Email;
                request.CustomerAddress1 = billingAddress.Line1;
                request.CustomerAddress2 = !string.IsNullOrEmpty(billingAddress.Line2) ? billingAddress.Line2 : billingAddress.Line1;
                request.CustomerPostcode = billingAddress.PostalCode;
                request.CustomerTown = billingAddress.City;
                request.CustomerCountry = ConvertThreeLetterNameToTwoLetterName(billingAddress.CountryCode);

                System.Text.RegularExpressions.Regex.IsMatch("123", @"^(\+[\-\s]?)\d{10}$");
                request.CustomerPhoneNumber = GetValidPhonenumber(billingAddress.DaytimePhoneNumber ?? billingAddress.EveningPhoneNumber);
            }
            return request;
        }

        /// <summary>
        /// Get a valid phone number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetValidPhonenumber(string value)
        {
            if (!Regex.IsMatch(value, @"^(\+[\-\s]?)\d{10}$"))
            {
                return string.Empty;
            }
            return value;
        }

        /// <summary>
        /// Convert three letter country code to two letters
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string ConvertThreeLetterNameToTwoLetterName(string name)
        {
            if (name.Length == 2)
            {
                return name;
            }
            if (name.Length != 3)
            {
                return null;
            }

            name = name.ToUpper();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo culture in cultures)
            {
                RegionInfo region = new RegionInfo(culture.LCID);
                if (region.ThreeLetterISORegionName.ToUpper() == name)
                {
                    return region.TwoLetterISORegionName;
                }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
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
        private readonly Injected<IOrderGroupTotalsCalculator> _orderGroupTotalsCalculator;
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(RegisterPaymentStep));

        public RegisterPaymentStep(IPayment payment) : base(payment)
        {
        }

        /// <summary>
        /// Process register payment step
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="orderForm"></param>
        /// <param name="orderGroup"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, ref string message)
        {
            var paymentMethodDto = PaymentManager.GetPaymentMethod(payment.PaymentMethodId);

            if (payment.TransactionType == "Authorization")
            {
                var transactionId = "";
                try
                {
                    transactionId = this.Client.Register(CreatePaymentRequest(paymentMethodDto, payment, orderForm, orderGroup));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    message = ex.Message;
                    AddNoteAndSaveChanges(orderGroup, "Payment Registered - Error", "Payment Registered - Error: " + ex.Message);
                    return false;
                }

                AddNoteAndSaveChanges(orderGroup, "Payment - Registered", "Payment - Registered");


                var url = new UriBuilder(GetTerminalUrl(paymentMethodDto));
                var nvc = new NameValueCollection
                    {
                        { "merchantId", paymentMethodDto.GetParameter(NetaxeptConstants.MerchantIdField) },
                        { "transactionId", transactionId }
                    };

                url.Query = string.Join("&", nvc.AllKeys.Select(key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));


                if (!HttpContext.Current.Response.IsRequestBeingRedirected)
                {
                    HttpContext.Current.Response.Redirect(url.ToString());
                }

                return true;
            }
            else if (Successor != null)
            {
                return Successor.Process(payment, orderForm, orderGroup, ref message);
            }
            return true;
        }

        /// <summary>
        /// Get terminal url for test or production
        /// </summary>
        /// <param name="paymentMethodDto"></param>
        /// <returns></returns>
        private string GetTerminalUrl(PaymentMethodDto paymentMethodDto)
        {
            var isProduction = bool.Parse(paymentMethodDto.GetParameter(NetaxeptConstants.IsProductionField, "false"));

            return (isProduction ? NetaxeptConstants.TerminalProductionUrl : NetaxeptConstants.TerminalTestUrl);
        }

        /// <summary>
        /// Create payment request
        /// </summary>
        /// <param name="paymentMethodDto"></param>
        /// <param name="payment"></param>
        /// <param name="orderForm"></param>
        /// <param name="orderGroup"></param>
        /// <returns></returns>
        private PaymentRequest CreatePaymentRequest(PaymentMethodDto paymentMethodDto, IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup)
        {
            var request = new PaymentRequest {EnableEasyPayments = true};

            if (orderGroup.CustomerId != Guid.Empty)
            {
                var customerContact = CustomerContext.Current.GetContactById(orderGroup.CustomerId);
                if (customerContact != null)
                {
                    request.PanHash = customerContact[NetaxeptConstants.CustomerPanHashFieldName]?.ToString();
                }
            }

            var calculatedTotals = _orderGroupTotalsCalculator.Service.GetTotals(orderGroup);

            request.Amount = PaymentStepHelper.GetAmount(payment.Amount);
            request.TaxTotal = PaymentStepHelper.GetAmount(calculatedTotals.TaxTotal.Amount);
            request.CurrencyCode = orderGroup.Currency.CurrencyCode;
            request.OrderDescription = "Netaxept order";
            request.OrderNumber = CartOrderNumberHelper.GenerateOrderNumber(orderGroup);
            
            request.Language = paymentMethodDto.GetParameter(NetaxeptConstants.TerminalLanguageField);

            request.SuccessUrl = payment.Properties.GetStringValue(NetaxeptConstants.CallbackUrl, string.Empty);

            request.CustomerNumber = (orderGroup.CustomerId != Guid.Empty ? orderGroup.CustomerId.ToString() : string.Empty);

            var billingAddress = payment.BillingAddress;
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

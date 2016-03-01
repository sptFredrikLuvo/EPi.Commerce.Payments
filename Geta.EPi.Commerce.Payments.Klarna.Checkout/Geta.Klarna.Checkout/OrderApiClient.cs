using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Klarna.Checkout.Models;
using Klarna.Api;
using Encoding = Klarna.Api.Encoding;

namespace Geta.Klarna.Checkout
{
    public class OrderApiClient
    {
        public int MerchantId { get; private set; }
        public string SharedSecret { get; private set; }
        public Locale Locale { get; private set; }
        public bool IsLiveMode { get; private set; }

        public OrderApiClient(int merchantId, string sharedSecret, Locale currentLocale, bool isLiveMode)
        {
            MerchantId = merchantId;
            SharedSecret = sharedSecret;
            Locale = currentLocale;
            IsLiveMode = isLiveMode;
        }

        /// <summary>
        /// Runs capture, updates order id and handles partial shipment
        /// </summary>
        /// <param name="reservationNumber"></param>
        /// <param name="transactionId"></param>
        /// <param name="orderId"></param>
        /// <param name="cartItems"></param>
        /// <returns></returns>
        public ActivateResponse Activate(string reservationNumber, string transactionId, string orderId, List<ICartItem> cartItems)
        {
            var result = new ActivateResponse(string.Empty, RiskStatus.Undefined, transactionId);
            
            try
            {
                var api = new Api(CurrentConfiguration);
                api.UpdateCart(cartItems);
                api.OrderId1 = orderId;
                api.Update(reservationNumber); // update with order id and cart items

                var response = api.Activate(reservationNumber);

                return new ActivateResponse(response.InvoiceNumber, response.RiskStatus, transactionId)
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                SetError(result, ex);
            }

            return result;
        }

        public bool CancelReservation(string reservationNumber)
        {
             var api = new Api(CurrentConfiguration);
             return api.CancelReservation(reservationNumber);
        }

        public RefundResponse HandleRefund(string invoiceNumber, List<ICartItem> cartItems)
        {
            var result = new RefundResponse();
            try
            {
                var api = new Api(CurrentConfiguration);

                foreach (var cartItem in cartItems)
                {
                    api.AddArticleNumber(cartItem.Quantity, cartItem.Reference);
                }
                result.InvoiceNumber = api.CreditPart(invoiceNumber);
                result.IsSuccess = true;
                return result;

            }
            catch (Exception ex)
            {
                SetError(result, ex);
            }
            return result;
        }

        public RefundResponse CreditInvoice(string invoiceNumber)
        {
            var result = new RefundResponse();
            try
            {
                var api = new Api(CurrentConfiguration);
                result.InvoiceNumber = api.CreditInvoice(invoiceNumber);
                result.IsSuccess = true;
                return result;

            }
            catch (Exception ex)
            {
                SetError(result, ex);
            }
            return result;
        }

        private void SetError(IResult result, Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.ErrorCode = ex.HResult;
        }


        private Configuration _currentConfiguration;
        private Configuration CurrentConfiguration
        {
            get
            {
                if (_currentConfiguration == null)
                    _currentConfiguration = GetConfiguration();

                return _currentConfiguration;
            }
        }


        // There is not good way of retriving supported Configurations from Klarna API
        // Try to load configuration based on Locale provider setting
        internal Configuration GetConfiguration()
        {
            var purchaseCountry = Locale.PurchaseCountry.ToUpper();
            Configuration config = null;

            if (purchaseCountry == "NO")
            {
                config = new Configuration(Country.Code.NO, Language.Code.NB, Currency.Code.NOK,
                    Encoding.Norway);
            }
            else if (purchaseCountry == "SE")
            {
                config = new Configuration(Country.Code.SE, Language.Code.SV, Currency.Code.SEK,
                    Encoding.Sweden);
            }
            else if (purchaseCountry == "FI")
            {
                config = new Configuration(Country.Code.FI, Language.Code.FI, Currency.Code.EUR,
                    Encoding.Finland);
            }
            else if (purchaseCountry == "DE")
            {
                config = new Configuration(Country.Code.DE, Language.Code.DE, Currency.Code.EUR,
                    Encoding.Germany);
            }
            else if (purchaseCountry == "DA")
            {
                config = new Configuration(Country.Code.DK, Language.Code.DA, Currency.Code.DKK,
                    Encoding.Denmark);
            }
            else { 
                // default to Sweden
                config = new Configuration(Country.Code.SE, Language.Code.SV, Currency.Code.SEK,
                    Encoding.Sweden);
            }
            config.Secret = SharedSecret;
            config.Eid = MerchantId;
            config.IsLiveMode = IsLiveMode;

            return config;
        }
        
        
    }
}

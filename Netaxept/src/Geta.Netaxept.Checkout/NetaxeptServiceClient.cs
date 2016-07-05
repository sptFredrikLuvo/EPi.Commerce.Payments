
using System;
using System.Configuration;
using Geta.Netaxept.Checkout.Models;
using Geta.Netaxept.Checkout.NetaxeptWebServiceClient;
using Environment = Geta.Netaxept.Checkout.NetaxeptWebServiceClient.Environment;

namespace Geta.Netaxept.Checkout
{
    public class NetaxeptServiceClient
    {
        private readonly NetaxeptClient _client;

        /// <summary>
        /// Public constructor
        /// </summary>
        public NetaxeptServiceClient()
        {
            _client = new NetaxeptClient();
        }

        public string Register(PaymentRequest paymentRequest)
        {
            if (paymentRequest == null)
            {
                throw new ArgumentNullException(nameof(paymentRequest));
            }

            var registerRequest = new RegisterRequest
            {
                Terminal = new Terminal
                {
                    OrderDescription = paymentRequest.OrderDescription,
                    RedirectUrl = paymentRequest.SuccessUrl,
                    Language = paymentRequest.Language 
                },
                Order = new Order
                {
                    Amount = paymentRequest.Amount,
                    CurrencyCode = paymentRequest.CurrencyCode,
                    OrderNumber = paymentRequest.OrderNumber
                },
                Environment = new Environment
                {
                    WebServicePlatform = "WCF"
                },
                Customer = new Customer
                {
                    CustomerNumber = paymentRequest.CustomerNumber,
                    FirstName = paymentRequest.CustomerFirstname,
                    LastName = paymentRequest.CustomerLastname,
                    Email = paymentRequest.CustomerEmail,
                    Address1 = paymentRequest.CustomerAddress1,
                    Address2 = paymentRequest.CustomerAddress2,
                    Postcode = paymentRequest.CustomerPostcode,
                    Town = paymentRequest.CustomerTown,
                    Country = paymentRequest.CustomerCountry,
                    PhoneNumber = paymentRequest.CustomerPhoneNumber
                }
            };

            if (paymentRequest.EnableEasyPayments)
            {
                registerRequest.Recurring = new Recurring
                {
                    PanHash = paymentRequest.PanHash,
                    Type = "S"
                };
            }

            var response = _client.Register(paymentRequest.MerchantId, paymentRequest.Token, registerRequest);

            return response.TransactionId;
        }

        public PaymentResult Query(string merchantId, string token, string transactionId)
        {
            var response = _client.Query(merchantId, token, new QueryRequest
            {
                TransactionId = transactionId
            });

            var paymentInfo = (PaymentInfo)response;

            return Create(paymentInfo);
        }

        private PaymentResult Create(PaymentInfo info)
        {
            return new PaymentResult
            {
                CardInformationPanHash = info.CardInformation.PanHash,
                CardInformationMaskedPan = info.CardInformation.MaskedPAN,
                CardInformationIssuer = info.CardInformation.Issuer,
                CardInformationExpiryDate = info.CardInformation.ExpiryDate,
                CardInformationIssuerCountry = info.CardInformation.IssuerCountry,
                CardInformationIssuerId = info.CardInformation.IssuerId,
                CardInformationPaymentMethod = info.CardInformation.PaymentMethod,

                Cancelled = (info.Error != null ? info.Error.ResponseCode.Equals("17") : false),
                ErrorOccurred = (info.Error != null),
                ErrorMessage = (info.Error != null ? string.Format("{0}/{1}: {2}", info.Error.ResponseCode, info.Error.ResponseSource, info.Error.ResponseText) : string.Empty)
            };
        }
    }
}

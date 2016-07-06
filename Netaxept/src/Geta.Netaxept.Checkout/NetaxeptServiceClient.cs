using System;
using Geta.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout.Models;
using Geta.Netaxept.Checkout.NetaxeptWebServiceClient;
using Environment = Geta.Netaxept.Checkout.NetaxeptWebServiceClient.Environment;

namespace Geta.Netaxept.Checkout
{
    /// <summary>
    /// Client for the communication with the Netaxept service
    /// </summary>
    public class NetaxeptServiceClient
    {
        private readonly NetaxeptClient _client;
        private readonly ClientConnection _connection;

        /// <summary>
        /// Public constructor
        /// </summary>
        public NetaxeptServiceClient(ClientConnection connection)
        {
            _connection = connection;
            _client = new NetaxeptClient();
        }

        /// <summary>
        /// Reqister payment at Netaxept. This method will return the transaction id
        /// </summary>
        /// <param name="paymentRequest"></param>
        /// <returns></returns>
        public string Register(PaymentRequest paymentRequest)
        {
            if (paymentRequest == null)
            {
                throw new ArgumentNullException(nameof(paymentRequest));
            }

            paymentRequest.Validate();

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

            var response = _client.Register(_connection.MerchantId, _connection.Token, registerRequest);

            return response.TransactionId;
        }

        /// <summary>
        /// Execute query method
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public PaymentResult Query(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }

            var response = _client.Query(_connection.MerchantId, _connection.Token, new QueryRequest
            {
                TransactionId = transactionId
            });

            var paymentInfo = (PaymentInfo)response;

            return Create(paymentInfo);
        }

        /// <summary>
        /// Execute sale method
        /// </summary>
        /// <param name="transactionId"></param>
        public void Sale(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }

            var response = _client.Process(_connection.MerchantId, _connection.Token, new ProcessRequest
            {
                Operation = "SALE",
                TransactionId = transactionId
            });
        }

        /// <summary>
        /// Execute capture method
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public void Capture(string transactionId, string amount)
        {
            var response = _client.Process(_connection.MerchantId, _connection.Token, new ProcessRequest
            {
                Operation = "CAPTURE",
                TransactionId = transactionId,
                TransactionAmount = amount,
            });
        }

        /// <summary>
        /// Execute authorize method
        /// </summary>
        /// <param name="transactionId"></param>
        public void Authorize(string transactionId)
        {
            var response = _client.Process(_connection.MerchantId, _connection.Token, new ProcessRequest
            {
                Operation = "AUTH",
                TransactionId = transactionId,
            });
        }

        /// <summary>
        /// Execute credit method
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public void Credit(string transactionId, string amount)
        {
            var response = _client.Process(_connection.MerchantId, _connection.Token, new ProcessRequest
            {
                Operation = "CREDIT",
                TransactionId = transactionId,
                TransactionAmount = amount
            });
        }

        /// <summary>
        /// Create payment result object
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
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

                AmountCaptured = int.Parse(info.Summary.AmountCaptured),

                Cancelled = (info.Error != null ? info.Error.ResponseCode.Equals("17") : false),
                ErrorOccurred = (info.Error != null),
                ErrorMessage = (info.Error != null ? string.Format("{0}/{1}: {2}", info.Error.ResponseCode, info.Error.ResponseSource, info.Error.ResponseText) : string.Empty)
            };
        }
    }
}

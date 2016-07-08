using System;
using System.ServiceModel;
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

            if (connection.IsProduction)
            {
                _client.Endpoint.Address = new EndpointAddress(NetaxeptConstants.NetaxeptServiceProductionAddress);
            }
            else
            {
                _client.Endpoint.Address = new EndpointAddress(NetaxeptConstants.NetaxeptServiceTestAddress);
            }
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
                    Language = paymentRequest.Language,
                    Vat = paymentRequest.TaxTotal
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
        public ProcessResult Sale(string transactionId)
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
            return Create(response);
        }

        /// <summary>
        /// Execute capture method
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public ProcessResult Capture(string transactionId, string amount)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentNullException(nameof(amount));
            }
            var response = _client.Process(_connection.MerchantId, _connection.Token, new ProcessRequest
            {
                Operation = "CAPTURE",
                TransactionId = transactionId,
                TransactionAmount = amount,
            });
            return Create(response);
        }

        /// <summary>
        /// Execute authorize method
        /// </summary>
        /// <param name="transactionId"></param>
        public ProcessResult Authorize(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            var response = _client.Process(_connection.MerchantId, _connection.Token, new ProcessRequest
            {
                Operation = "AUTH",
                TransactionId = transactionId,
            });
            return Create(response);
        }

        /// <summary>
        /// Execute credit method
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public ProcessResult Credit(string transactionId, string amount)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentNullException(nameof(amount));
            }
            var response = _client.Process(_connection.MerchantId, _connection.Token, new ProcessRequest
            {
                Operation = "CREDIT",
                TransactionId = transactionId,
                TransactionAmount = amount
            });
            return Create(response);
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
                ErrorCode = (info.Error != null ? info.Error.ResponseCode : string.Empty),
                ErrorSource = (info.Error != null ? info.Error.ResponseSource : string.Empty),
                ErrorMessage = (info.Error != null ? string.Format("{0}/{1}: {2}", info.Error.ResponseCode, info.Error.ResponseSource, info.Error.ResponseText) : string.Empty)
            };
        }

        /// <summary>
        /// Create ProcessResult object from response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private ProcessResult Create(ProcessResponse response)
        {
            return new ProcessResult
            {
                ErrorOccurred = (response.ResponseCode != "OK"),
                ErrorMessage = (response.ResponseCode != "OK" ? string.Format("{0}/{1}: {2}", response.ResponseCode, response.ResponseSource, response.ResponseText) : string.Empty),
                ResponseCode = response.ResponseCode,
                ResponseText = response.ResponseText,
                ResponseSource = response.ResponseSource
            };
        }
    }
}

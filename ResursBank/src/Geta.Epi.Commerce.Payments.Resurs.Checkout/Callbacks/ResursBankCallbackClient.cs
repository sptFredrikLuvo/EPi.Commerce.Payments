using System;
using System.Configuration;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Epi.Commerce.Payments.Resurs.Checkout.Extensions;
using Geta.EPi.Commerce.Payments.Resurs.Checkout.Extensions;
using Geta.Resurs.Checkout;
using Geta.Resurs.Checkout.Callbacks;
using Geta.Resurs.Checkout.ConfigurationService;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Callbacks
{
    public class ResursBankCallbackClient : IResursBankCallbackClient
    {
        private Injected<IResursHashCalculator> InjectedHashCalculator { get; set; }
        private static readonly Injected<IOrderRepository> _orderRepository;

        private readonly ConfigurationWebServiceClient _configurationService;

        public ResursBankCallbackClient(ResursCredential credential)
        {
            _configurationService = new ConfigurationWebServiceClient();
            if (_configurationService.ClientCredentials != null)
            {
                if (credential != null)
                {
                    _configurationService.ClientCredentials.UserName.UserName = credential.UserName;
                    _configurationService.ClientCredentials.UserName.Password = credential.Password;
                }
                else
                {
                    var appSettings = ConfigurationManager.AppSettings;
                    _configurationService.ClientCredentials.UserName.UserName = appSettings["ResursBank:UserName"] ?? "Not Found";
                    _configurationService.ClientCredentials.UserName.Password = appSettings["ResursBank:Password"] ?? "Not Found";
                }
            }
        }

        public void RegisterCallbackUrl(CallbackEventType callbackEventType, string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            var urlWithEventType = GetUrlWithEventType(callbackEventType, url);

            var currentCallback =
                _configurationService.getRegisteredEventCallback(callbackEventType.ToString());

            if (!string.IsNullOrWhiteSpace(currentCallback) && !currentCallback.Equals(urlWithEventType, StringComparison.InvariantCultureIgnoreCase))
            {
                _configurationService.unregisterEventCallback(callbackEventType.ToString());
            }

            _configurationService.registerEventCallback(
                callbackEventType.ToString(),
                urlWithEventType,
                null,
                null,
                new digestConfiguration
                {
                    digestAlgorithm = GetDigestAlgorithm(),
                    digestParameters = GetDigestParameters(),
                    digestSalt = GetSalt()
                });
        }

        public void UnRegisterCallbackUrl(CallbackEventType callbackEventType)
        {
            var currentCallback =
                _configurationService.getRegisteredEventCallback(callbackEventType.ToString());

            if (!string.IsNullOrWhiteSpace(currentCallback))
            {
                _configurationService.unregisterEventCallback(callbackEventType.ToString());
            }
        }

        public bool ProcessCallback(CallbackData callbackData, string digest)
        {
            if (callbackData == null) throw new ArgumentNullException(nameof(callbackData));
            if (digest == null) throw new ArgumentNullException(nameof(digest));

            if (!CheckDigest(callbackData, digest))
            {
                throw new ArgumentException(nameof(digest));
            }

            // Get order
            var order = GetOrderByPayment(callbackData.PaymentId);
            if (order == null)
            {
                return false;
            }
            var payment = GetPayment(callbackData, order);
            if (payment == null)
            {
                return false;
            }

            switch (callbackData.EventType)
            {
                case CallbackEventType.UNFREEZE:
                    OrderStatusManager.ReleaseHoldOnOrder(order);
                    payment.Status = PaymentStatus.Processed.ToString();
                    payment.AcceptChanges();
                    break;
                case CallbackEventType.ANNULMENT:
                    OrderStatusManager.CancelOrder(order);
                    payment.Status = PaymentStatus.Failed.ToString();
                    payment.AcceptChanges();
                    break;
            }

            // Add order note
            var message = $"ResursBankCallback: {callbackData.EventType}";
            order.AddNote(message, message);

            return true;
        }

        private Payment GetPayment(CallbackData callbackData, PurchaseOrder order)
        {
            return order.OrderForms.SelectMany(x => x.Payments)
                .FirstOrDefault(payment => payment.GetStringValue(ResursConstants.ResursPaymentId, string.Empty)
                    .Equals(callbackData.PaymentId));
        }

        protected virtual PurchaseOrder GetOrderByPayment(string paymentId)
        {
            var searchOptions = new OrderSearchOptions
            {
                CacheResults = false,
                StartingRecord = 0,
                RecordsToRetrieve = 1
            };
            searchOptions.Classes.Add("PurchaseOrder");

            var parameters = new OrderSearchParameters
            {
                SqlWhereClause = "OrderGroupId IN " +
                                 "(SELECT OrderGroupId FROM [OrderFormPayment] WHERE [PaymentId] IN " +
                                 "(SELECT ObjectId FROM [OrderFormPayment_Other] WHERE [ResursPaymentId] = " +
                                $"'{paymentId}'" +
                                 "))"
            };

            var purchaseOrderCollection = OrderContext.Current.FindPurchaseOrders(parameters, searchOptions);
            return purchaseOrderCollection.FirstOrDefault();
        }

        protected virtual string GetUrlWithEventType(CallbackEventType callbackEventType, string url)
        {
            return url.Replace("{eventType}", callbackEventType.ToString());
        }

        public virtual bool CheckDigest(CallbackData callbackData, string digest)
        {
            if (string.IsNullOrWhiteSpace(digest))
            {
                return false;
            }
            return digest.Equals(InjectedHashCalculator.Service.Compute(callbackData, GetSalt()));
        }

        protected virtual digestAlgorithm GetDigestAlgorithm()
        {
            return digestAlgorithm.SHA1;
        }

        protected virtual string GetSalt()
        {
            return ConfigurationManager.AppSettings["ResursBank:CallbackDigestSalt"];
        }

        protected virtual string[] GetDigestParameters()
        {
            return new[] { "paymentId" };
        }
    }
}
using System;
using System.Configuration;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout.ConfigurationService;
using Geta.Resurs.Checkout.Model;

namespace Geta.Resurs.Checkout.Callbacks
{
    public class ResursBankCallbackClient : IResursBankCallbackClient
    {
        private Injected<IResursHashCalculator> InjectedHashCalculator { get; set; }
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

            // get order
            // update order

            return false;
        }

        protected virtual string GetUrlWithEventType(CallbackEventType callbackEventType, string url)
        {
            return url.Contains("?")
                ? $"{url}&eventType={callbackEventType.ToString().ToUpper()}"
                : $"{url}?eventType={callbackEventType.ToString().ToUpper()}";
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
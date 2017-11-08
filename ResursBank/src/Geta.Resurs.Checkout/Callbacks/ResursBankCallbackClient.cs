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
            var currentCallback =
                _configurationService.getRegisteredEventCallback(callbackEventType.ToString());

            if (!string.IsNullOrWhiteSpace(currentCallback) && !currentCallback.Equals(url, StringComparison.InvariantCultureIgnoreCase))
            {
                _configurationService.unregisterEventCallback(callbackEventType.ToString());
            }

            _configurationService.registerEventCallback(
                callbackEventType.ToString(),
                url,
                string.Empty,
                string.Empty,
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
            if (!CheckDigest(callbackData, digest))
            {
                return false;
            }

            // get order
            // update order

            return false;
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
using System;
using System.Configuration;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.Resurs.Checkout.Callbacks;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ResursCallbackInitialization : IInitializableModule
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(ResursCallbackInitialization));
        private static bool _initialized;

        public void Initialize(InitializationEngine context)
        {
            if (_initialized)
            {
                return;
            }

            var callbackUrl = ConfigurationManager.AppSettings["ResursBank:CallbackUrl"];
            
            try
            {
                var configurationClient = GetResursBankConfigurationClient();
                if (!string.IsNullOrWhiteSpace(callbackUrl))
                {
                    configurationClient.RegisterCallbackUrl(CallbackEventType.UNFREEZE, callbackUrl);
                    configurationClient.RegisterCallbackUrl(CallbackEventType.ANNULMENT, callbackUrl);
                }
                else
                {
                    _logger.Error("ResursCallbackInitialization ResursBank:CallbackUrl is empty, unregistering callbacks");

                    configurationClient.UnRegisterCallbackUrl(CallbackEventType.UNFREEZE);
                    configurationClient.UnRegisterCallbackUrl(CallbackEventType.ANNULMENT);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("ResursCallbackInitialization failed", ex);
            }
            
            _initialized = true;
        }

        private IResursBankCallbackClient GetResursBankConfigurationClient()
        {
            var factory = ServiceLocator.Current.GetInstance<IResursBankCallbackClientFactory>();
            return factory.Init();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
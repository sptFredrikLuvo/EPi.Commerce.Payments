using System.Configuration;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Geta.Resurs.Checkout.Callbacks
{
    [InitializableModule]
    [ModuleDependency(typeof(InitializationModule))]
    public class ResursCallbackInitialization : IInitializableModule
    {
        private static bool _initialized;

        public void Initialize(InitializationEngine context)
        {
            if (_initialized)
            {
                return;
            }

            var callbackUrl = ConfigurationManager.AppSettings["ResursBank:CallbackUrl"];
            var configurationClient = GetResursBankConfigurationClient();

            if (!string.IsNullOrWhiteSpace(callbackUrl))
            {
                configurationClient.RegisterCallbackUrl(CallbackEventType.UNFREEZE, callbackUrl);
                configurationClient.RegisterCallbackUrl(CallbackEventType.ANNULMENT, callbackUrl);
            }
            else
            {
                configurationClient.UnRegisterCallbackUrl(CallbackEventType.UNFREEZE);
                configurationClient.UnRegisterCallbackUrl(CallbackEventType.ANNULMENT);
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
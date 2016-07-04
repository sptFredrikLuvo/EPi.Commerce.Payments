using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class MetadataInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));
        
        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;

            var netaxeptBankPaymentMethod = GetOrCreateNetaxeptPaymentMethodField(mdContext);
            JoinField(mdContext, netaxeptBankPaymentMethod, NetaxeptConstants.OtherPaymentClass);

        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        private MetaField GetOrCreateNetaxeptPaymentMethodField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, NetaxeptConstants.NetaxeptPaymentMethod);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Netaxept integration.", NetaxeptConstants.NetaxeptPaymentMethod));
                f = MetaField.Create(mdContext, NetaxeptConstants.OrderNamespace, NetaxeptConstants.NetaxeptPaymentMethod, NetaxeptConstants.NetaxeptPaymentMethod, string.Empty, MetaDataType.ShortString, 255, true, false, false, false);
            }

            return f;
        }

        private void JoinField(MetaDataContext mdContext, MetaField field, string metaClassName)
        {
            var cls = MetaClass.Load(mdContext, metaClassName);

            if (MetaFieldIsNotConnected(field, cls))
            {
                cls.AddField(field);
            }
        }

        private static bool MetaFieldIsNotConnected(MetaField field, MetaClass cls)
        {
            return cls != null && !cls.MetaFields.Contains(field);
        }
    }
}

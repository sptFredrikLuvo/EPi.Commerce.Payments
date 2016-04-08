using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.Commerce.Payments.Verifone.HostedPages
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class MetadataInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));

        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;
            var filingCode = GetOrCreateMetaField(mdContext, MetadataConstants.FilingCode, MetaDataType.BigInt);
            JoinField(mdContext, filingCode, MetadataConstants.PurchaseOrderClass);

            var referenceNumber = GetOrCreateMetaField(mdContext, MetadataConstants.ReferenceNumber, MetaDataType.BigInt);
            JoinField(mdContext, referenceNumber, MetadataConstants.PurchaseOrderClass);

            var transactionNumber = GetOrCreateMetaField(mdContext, MetadataConstants.TransactionNumber, MetaDataType.BigInt);
            JoinField(mdContext, transactionNumber, MetadataConstants.PurchaseOrderClass);
        }

        private MetaField GetOrCreateMetaField(MetaDataContext mdContext, string fieldName, MetaDataType metadataType)
        {
            var f = MetaField.Load(mdContext, fieldName);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Verifone payment integration.", fieldName));
                f = MetaField.Create(mdContext, MetadataConstants.OrderNamespace, fieldName, fieldName, string.Empty, metadataType, 0, true, false, false, false);
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

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}

using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class MetadataInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));

        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;
            var reservation = GetOrCreateReservationField(mdContext);
            JoinField(mdContext, reservation, MetadataConstants.OtherPaymentClass);

            var invoiceNumber = GetOrCreateInvoiceField(mdContext);
            JoinField(mdContext, invoiceNumber, MetadataConstants.PurchaseOrderClass);
        }

        private MetaField GetOrCreateReservationField(MetaDataContext mdContext)
        {
            
            var f = MetaField.Load(mdContext, MetadataConstants.ReservationField);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Klarna integration.", MetadataConstants.ReservationField));
                f = MetaField.Create(mdContext, MetadataConstants.OrderNamespace, MetadataConstants.ReservationField, MetadataConstants.ReservationField, string.Empty, MetaDataType.Int, 0, true, false, false, false);
            }

            return f;
        }

        private MetaField GetOrCreateInvoiceField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, MetadataConstants.InvoiceNumber);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Klarna integration.", MetadataConstants.InvoiceNumber));
                f = MetaField.Create(mdContext, MetadataConstants.OrderNamespace, MetadataConstants.InvoiceNumber, MetadataConstants.InvoiceNumber, string.Empty, MetaDataType.Int, 0, true, false, false, false);
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

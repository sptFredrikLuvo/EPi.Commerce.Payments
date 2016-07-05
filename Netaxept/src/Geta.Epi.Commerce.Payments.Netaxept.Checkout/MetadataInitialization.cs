using System;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Geta.Netaxept.Checkout;
using Mediachase.BusinessFoundation.Data;
using Mediachase.BusinessFoundation.Data.Meta.Management;
using Mediachase.Commerce.Catalog;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using MetaClass = Mediachase.MetaDataPlus.Configurator.MetaClass;
using MetaField = Mediachase.MetaDataPlus.Configurator.MetaField;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout
{
    /// <summary>
    /// Initialization module. 
    /// Create fields for the CustomerContact and for the payment method
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class MetadataInitialization : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MetadataInitialization));
        
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(InitializationEngine context)
        {
            MetaDataContext mdContext = CatalogContext.MetaDataContext;

            var netaxeptBankPaymentMethod = GetOrCreateNetaxeptPaymentMethodField(mdContext);
            JoinField(mdContext, netaxeptBankPaymentMethod, NetaxeptConstants.OtherPaymentClass);

            var successUrl = GetOrCreateSuccessUrlField(mdContext);
            JoinField(mdContext, successUrl, NetaxeptConstants.OtherPaymentClass);
            

            // Create PanHash field on the customer contact. We need this for using EasyPayments
            var customContactMetaClass = DataContext.Current.MetaModel.MetaClasses["Contact"];
            if (customContactMetaClass.Fields[NetaxeptConstants.CustomerPanHashFieldName] == null)
            {
                using (MetaFieldBuilder builder = new MetaFieldBuilder(customContactMetaClass))
                {
                    builder.CreateText(NetaxeptConstants.CustomerPanHashFieldName, NetaxeptConstants.CustomerPanHashFieldName, true, 100, false);
                    builder.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Unitialize
        /// </summary>
        /// <param name="context"></param>
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

        private MetaField GetOrCreateSuccessUrlField(MetaDataContext mdContext)
        {

            var f = MetaField.Load(mdContext, NetaxeptConstants.SuccessfullUrl);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Resurs integration.", NetaxeptConstants.SuccessfullUrl));
                f = MetaField.Create(mdContext, NetaxeptConstants.OrderNamespace, NetaxeptConstants.SuccessfullUrl, NetaxeptConstants.SuccessfullUrl, string.Empty, MetaDataType.LongString, Int32.MaxValue, true, false, false, false);
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

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

            var netaxeptBankPaymentMethod = GetOrCreateCardField(mdContext, NetaxeptConstants.NetaxeptPaymentMethod);
            JoinField(mdContext, netaxeptBankPaymentMethod, NetaxeptConstants.OtherPaymentClass);

            var callbackUrl = GetOrCreateCardField(mdContext, NetaxeptConstants.CallbackUrl);
            JoinField(mdContext, callbackUrl, NetaxeptConstants.OtherPaymentClass);

            var cardInformationIssuerCountryField = GetOrCreateCardField(mdContext, NetaxeptConstants.CardInformationIssuerCountryField);
            JoinField(mdContext, cardInformationIssuerCountryField, NetaxeptConstants.OtherPaymentClass);

            var cardInformationExpiryDateField = GetOrCreateCardField(mdContext, NetaxeptConstants.CardInformationExpiryDateField);
            JoinField(mdContext, cardInformationExpiryDateField, NetaxeptConstants.OtherPaymentClass);

            var cardInformationIssuerField = GetOrCreateCardField(mdContext, NetaxeptConstants.CardInformationIssuerField);
            JoinField(mdContext, cardInformationIssuerField, NetaxeptConstants.OtherPaymentClass);

            var cardInformationIssuerIdField = GetOrCreateCardField(mdContext, NetaxeptConstants.CardInformationIssuerIdField);
            JoinField(mdContext, cardInformationIssuerIdField, NetaxeptConstants.OtherPaymentClass);

            var cardInformationMaskedPanField = GetOrCreateCardField(mdContext, NetaxeptConstants.CardInformationMaskedPanField);
            JoinField(mdContext, cardInformationMaskedPanField, NetaxeptConstants.OtherPaymentClass);

            var cardInformationPaymentMethodField = GetOrCreateCardField(mdContext, NetaxeptConstants.CardInformationPaymentMethodField);
            JoinField(mdContext, cardInformationPaymentMethodField, NetaxeptConstants.OtherPaymentClass);

            var cartOrderNumberTempField = GetOrCreateCardField(mdContext, NetaxeptConstants.CartOrderNumberTempField);
            JoinField(mdContext, cartOrderNumberTempField, NetaxeptConstants.CartClass);

            // Create PanHash field on the customer contact. We need this for using EasyPayments
            var customContactMetaClass = DataContext.Current.MetaModel.MetaClasses["Contact"];
            if (customContactMetaClass != null && customContactMetaClass.Fields[NetaxeptConstants.CustomerPanHashFieldName] == null)
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

        /// <summary>
        /// Get or create card field
        /// </summary>
        /// <param name="mdContext"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private MetaField GetOrCreateCardField(MetaDataContext mdContext, string fieldName)
        {

            var f = MetaField.Load(mdContext, fieldName);
            if (f == null)
            {
                Logger.Debug(string.Format("Adding meta field '{0}' for Netaxept integration.", fieldName));
                f = MetaField.Create(mdContext, NetaxeptConstants.OrderNamespace, fieldName, fieldName, string.Empty, MetaDataType.LongString, Int32.MaxValue, true, false, false, false);
            }
            return f;
        }

        /// <summary>
        /// Add field to meta class
        /// </summary>
        /// <param name="mdContext"></param>
        /// <param name="field"></param>
        /// <param name="metaClassName"></param>
        private void JoinField(MetaDataContext mdContext, MetaField field, string metaClassName)
        {
            var cls = MetaClass.Load(mdContext, metaClassName);

            if (MetaFieldIsNotConnected(field, cls))
            {
                cls.AddField(field);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="cls"></param>
        /// <returns></returns>
        private static bool MetaFieldIsNotConnected(MetaField field, MetaClass cls)
        {
            return cls != null && !cls.MetaFields.Contains(field);
        }
    }
}

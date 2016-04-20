using Geta.Commerce.Payments.Verifone.HostedPages.Models;
using Mediachase.Commerce.Storage;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Extensions
{
    public static class MetaStorageBaseExtensions
    {
        public static void SetInitializationMetaFields(this MetaStorageBase order, PaymentInitializationRequest initializationRequest)
        {
            order.SetMetaField(MetadataConstants.OrderTimestamp, initializationRequest.OrderTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            order.SetMetaField(MetadataConstants.OrderCurrencyCode, initializationRequest.OrderCurrencyCode);

            if (string.IsNullOrWhiteSpace(initializationRequest.SignatureOne) == false)
            {
                order.SetMetaField(MetadataConstants.SignatureOne, initializationRequest.SignatureOne);
            }

            if (string.IsNullOrWhiteSpace(initializationRequest.SignatureTwo) == false)
            {
                order.SetMetaField(MetadataConstants.SignatureTwo, initializationRequest.SignatureTwo);
            }
        }

        public static void SetSuccessMetaFields(this MetaStorageBase order, PaymentSuccessResponse successResponse)
        {
            //order.SetMetaField(MetadataConstants.FilingCode, long.Parse(successResponse.FilingCode));
            //order.SetMetaField(MetadataConstants.TransactionNumber, long.Parse(successResponse.TransactionNumber));
            //order.SetMetaField(MetadataConstants.ReferenceNumber, long.Parse(successResponse.ReferenceNumber));
        }
    }
}
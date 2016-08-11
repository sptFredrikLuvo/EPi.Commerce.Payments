using System.Linq;
using Mediachase.Commerce.Storage;
using Mediachase.MetaDataPlus;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions
{
    public static class MetaStorageExtensions
    {
        public static string GetStringValue(this MetaStorageBase item, string fieldName)
        {
            return item.GetStringValue(fieldName, string.Empty);
        }

        public static string GetStringValue(this MetaStorageBase item, string fieldName, string defaultValue)
        {
            return item.FieldExists(fieldName) && item[fieldName] != null ? item[fieldName].ToString() : defaultValue;
        }

        public static decimal GetDecimalValue(this MetaStorageBase item, string fieldName, decimal defaultValue)
        {
            if (item.FieldExists(fieldName) && item[fieldName] != null)
            {
                decimal val;
                if(decimal.TryParse(item[fieldName].ToString(), out val)) return val;
            }
            return defaultValue;
        }

        private static bool FieldExists(this MetaObject item, string fieldName)
        {
            return item.GetValues().Keys.Cast<string>().Contains(fieldName);
        }
    }
}

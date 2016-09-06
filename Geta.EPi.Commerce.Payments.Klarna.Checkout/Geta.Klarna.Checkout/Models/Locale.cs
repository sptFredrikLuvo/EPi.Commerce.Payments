using System.Collections.Generic;

namespace Geta.Klarna.Checkout.Models
{
    public class Locale
    {
        public string Country { get; private set; }
        public string Language { get; private set; }
        public string PurchaseCountry { get; private set; }
        public string PurchaseCurrency { get; private set; }
        public string LocaleCode { get; private set; }

        protected Locale(
            string country, 
            string language, 
            string purchaseCountry, 
            string purchaseCurrency, 
            string localeCode)
        {
            Country = country;
            Language = language;
            PurchaseCountry = purchaseCountry;
            PurchaseCurrency = purchaseCurrency;
            LocaleCode = localeCode;
        }

        public static readonly Locale Sweden = new Locale("Sweden", "Swedish", "SE", "SEK", "sv-se");
        public static readonly Locale Norway = new Locale("Norway", "Norwegian", "NO", "NOK", "nb-no");
        public static readonly Locale Finland = new Locale("Finland", "Finnish", "FI", "EUR", "fi-fi");
        public static readonly Locale Germany = new Locale("Germany", "German", "DE", "EUR", "de-de");
        public static readonly Locale Denmark = new Locale("Denmark", "Danish", "DA", "DKK", "da-dk");
        public static readonly Locale Austria = new Locale("Austria", "German", "AT", "EUR", "de-at");

        public static IEnumerable<Locale> Locales
        {
            get
            {
                yield return Sweden;
                yield return Norway;
                yield return Finland;
                yield return Germany;
                yield return Denmark;
                yield return Austria;
            }
        }
    }
}
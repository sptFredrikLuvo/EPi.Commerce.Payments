
namespace Geta.Netaxept.Checkout.Models
{
    public class PaymentRequest
    {
        /* CONNECTION SETTINGS */
        public string MerchantId { get; set; }
        public string Token { get; set; }

        public string SuccessUrl { get; set; }
        public string OrderDescription { get; set; }

        /// <summary>
        /// Valid options: no_NO, sv_SE, da_DK, de_DE, fi_FI, ru_RU, pl_PL, es_ES, it_IT, en_GB
        /// </summary>
        public string Language { get; set; }

        /* ORDER */
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string OrderNumber { get; set; }

        /* PAYMENT OPTIONS */
        public bool EnableEasyPayments { get; set; }
        public string PanHash { get; set; }
    }
}

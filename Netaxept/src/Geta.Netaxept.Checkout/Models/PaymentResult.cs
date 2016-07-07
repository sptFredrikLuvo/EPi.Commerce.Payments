using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Netaxept.Checkout.Models
{
    public class PaymentResult
    {
        public bool Cancelled { get; set; }
        public bool ErrorOccurred { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorSource { get; set; }

        /* Card information*/
        public string CardInformationPanHash { get; set; }
        public string CardInformationMaskedPan { get; set; }
        public string CardInformationIssuer { get; set; }
        public string CardInformationExpiryDate { get; set; }
        public string CardInformationIssuerCountry { get; set; }
        public string CardInformationIssuerId { get; set; }
        public string CardInformationPaymentMethod { get; set; }

        public int AmountCaptured { get; set; }

    }
}

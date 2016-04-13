using System.Web.Mvc;
using Geta.Commerce.Payments.Verifone.HostedPages.Mvc;
using Geta.Verifone;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Models
{
    [ModelBinder(typeof(VerifoneModelBinder))]
    public class PaymentSuccessResponse
    {
        [BindAlias(VerifoneConstants.ParameterName.TransactionNumber)]
        public string TransactionNumber { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.PaymentMethodCodeResponse)]
        public string PaymentMethodCode { get; set; }

        [BindAlias(VerifoneConstants.ParameterName.OrderNumber)]
        public string OrderNumber { get; set; }
    }
}
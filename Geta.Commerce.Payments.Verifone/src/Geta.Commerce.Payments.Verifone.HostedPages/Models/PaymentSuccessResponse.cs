using System.Web.Mvc;
using Geta.Commerce.Payments.Verifone.HostedPages.Mvc;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Models
{
    [ModelBinder(typeof(VerifoneModelBinder))]
    public class PaymentSuccessResponse
    {
         
    }
}
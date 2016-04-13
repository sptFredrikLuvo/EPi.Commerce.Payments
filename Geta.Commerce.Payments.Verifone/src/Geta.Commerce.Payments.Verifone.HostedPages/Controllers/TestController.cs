using System.Web.Mvc;
using Geta.Commerce.Payments.Verifone.HostedPages.Models;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            var model = new PaymentInitializationRequest
            {
                MerchantAgreementCode = "demo-merchant-agreement-code",
                OrderNumber = "12345678"
            };

            return View(model);
        }
    }
}
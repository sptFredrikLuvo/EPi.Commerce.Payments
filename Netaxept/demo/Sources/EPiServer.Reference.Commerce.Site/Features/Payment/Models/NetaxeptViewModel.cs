using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business;
using Mediachase.Commerce.Website;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class NetaxeptViewModel : PaymentMethodViewModel<NetaxeptCheckoutPaymentGateway>
    {
        public NetaxeptViewModel()
        {
            InitializeValues();
        }

        public void InitializeValues()
        {
        }

        public IPaymentMethodViewModel<IPaymentOption> Payment { get; set; }
    }
}
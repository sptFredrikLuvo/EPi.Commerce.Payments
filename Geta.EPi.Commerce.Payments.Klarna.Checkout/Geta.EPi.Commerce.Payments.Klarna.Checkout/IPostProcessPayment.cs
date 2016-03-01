using EPiServer.ServiceLocation;
using Geta.Klarna.Checkout.Models;
using Mediachase.Commerce.Orders;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    public interface IPostProcessPayment
    {
        void PostCapture(ActivateResponse response, Payment payment);
        void PostAnnul(bool result, Payment payment);
        void PostCredit(RefundResponse response, Payment payment);
    }

    // Default implementation that does nothing - 
    [ServiceConfiguration(typeof(IPostProcessPayment))]
    public class PostProcessPaymentDummy : IPostProcessPayment
    {
        public void PostCapture(ActivateResponse response, Payment payment)
        {
            
        }

        public void PostAnnul(bool result, Payment payment)
        {
            
        }

        public void PostCredit(RefundResponse response, Payment payment)
        {
            
        }
    }
}

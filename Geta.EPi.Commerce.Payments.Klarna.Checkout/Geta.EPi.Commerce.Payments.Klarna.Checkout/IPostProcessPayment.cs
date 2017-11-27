using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Klarna.Checkout.Models;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    public interface IPostProcessPayment
    {
        void PostCapture(ActivateResponse response, IPayment payment);
        void PostAnnul(bool result, IPayment payment);
        void PostCredit(RefundResponse response, IPayment payment);
    }

    // Default implementation that does nothing - 
    [ServiceConfiguration(typeof(IPostProcessPayment))]
    public class PostProcessPaymentDummy : IPostProcessPayment
    {   
        public void PostCapture(ActivateResponse response, IPayment payment)
        {
            
        }

        public void PostAnnul(bool result, IPayment payment)
        {
            
        }

        public void PostCredit(RefundResponse response, IPayment payment)
        {
            
        }
    }
}

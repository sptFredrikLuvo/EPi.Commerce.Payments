using EPiServer.ServiceLocation;
using Geta.Klarna.Checkout.Models;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    public interface IPostProcessPayment
    {
        void PostCapture(ActivateResponse response);
        void PostAnnul(bool result, string transactionId, string reservation);
        void PostCredit(bool result, string transactionId, string invoiceNumber);
    }

    // Default implementation that does nothing - 
    [ServiceConfiguration(typeof(IPostProcessPayment))]
    public class PostProcessPaymentDummy : IPostProcessPayment
    {
        public void PostCapture(ActivateResponse response)
        {
            
        }

        public void PostAnnul(bool result, string transactionId, string reservation)
        {
            
        }

        public void PostCredit(bool result, string transactionId, string invoiceNumber)
        {

        }
    }
}

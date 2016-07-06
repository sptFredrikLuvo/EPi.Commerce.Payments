using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    /// <summary>
    /// Payment step 
    /// </summary>
    public abstract class PaymentStep
    {
        protected NetaxeptServiceClient Client;
        protected PaymentStep Successor;

        protected PaymentStep()
        {
            Client = new NetaxeptServiceClient();
        }

        public void SetSuccessor(PaymentStep successor)
        {
            this.Successor = successor;
        }

        /// <summary>
        /// Process payment step
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool Process(Payment payment, ref string message);
    }
}

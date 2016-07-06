using System;
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

        /// <summary>
        /// Add note to order
        /// </summary>
        /// <param name="orderForm"></param>
        /// <param name="title"></param>
        /// <param name="detail"></param>
        public void AddNote(OrderForm orderForm, string title, string detail)
        {
            OrderNote on = orderForm.Parent.OrderNotes.AddNew();
            on.Detail = detail;
            on.Title = title;
            on.Type = "Payments";
            on.Created = DateTime.Now;
        }
    }
}

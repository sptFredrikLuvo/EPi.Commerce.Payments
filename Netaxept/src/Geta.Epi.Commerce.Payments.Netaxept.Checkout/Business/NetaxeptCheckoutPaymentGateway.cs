
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Website;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    public class NetaxeptCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentOption
    {
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            throw new System.NotImplementedException();
        }

        public bool ValidateData()
        {
            throw new System.NotImplementedException();
        }

        public Payment PreProcess(OrderForm form)
        {
            throw new System.NotImplementedException();
        }

        public bool PostProcess(OrderForm orderForm)
        {
            throw new System.NotImplementedException();
        }
    }
}

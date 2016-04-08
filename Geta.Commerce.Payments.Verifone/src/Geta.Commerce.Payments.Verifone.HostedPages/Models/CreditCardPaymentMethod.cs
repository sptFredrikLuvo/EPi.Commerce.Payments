using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Website;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Models
{
    public class CreditCardPaymentMethod : IPaymentOption
    {
        public bool ValidateData()
        {
            return true;
        }

        public Payment PreProcess(OrderForm form)
        {
            return null;
        }

        public bool PostProcess(OrderForm orderForm)
        {
            return true;
        }
    }
}
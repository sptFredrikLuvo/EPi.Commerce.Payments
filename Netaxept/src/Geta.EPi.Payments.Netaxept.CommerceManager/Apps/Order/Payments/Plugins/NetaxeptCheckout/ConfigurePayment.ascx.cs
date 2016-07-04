using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
using Geta.Netaxept.Checkout;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Geta.EPi.Payments.Netaxept.CommerceManager.Apps.Order.Payments.Plugins.NetaxeptCheckout
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        public string ValidationGroup { get; set; }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;

            if (paymentMethod == null)
            {
                return;
            }
            txtMerchantId.Text = paymentMethod.GetParameter(NetaxeptConstants.MerchantId, string.Empty);
            txtToken.Text = paymentMethod.GetParameter(NetaxeptConstants.Token, string.Empty);
        }

        public void SaveChanges(object dto)
        {
            if (!Visible)
            {
                return;
            }

            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
            {
                return;
            }
            paymentMethod.SetParameter(NetaxeptConstants.MerchantId, txtMerchantId.Text);
            paymentMethod.SetParameter(NetaxeptConstants.Token, txtToken.Text);
        }
    }
}
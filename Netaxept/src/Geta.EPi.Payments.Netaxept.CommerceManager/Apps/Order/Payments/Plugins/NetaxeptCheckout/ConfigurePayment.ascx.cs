using Geta.Epi.Commerce.Payments.Netaxept.Checkout.Extensions;
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

            //txtUserName.Text = paymentMethod.GetParameter(ResursConstants.UserName, string.Empty);
            //txtPassword.Text = paymentMethod.GetParameter(ResursConstants.Password, string.Empty);
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

            //paymentMethod.SetParameter(ResursConstants.UserName, txtUserName.Text);
            //paymentMethod.SetParameter(ResursConstants.Password, txtPassword.Text);
        }
    }
}
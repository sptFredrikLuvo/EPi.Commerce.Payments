using System;
using System.Linq;
using System.Web.UI.WebControls;
using Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions;
using Geta.Klarna.Checkout.Models;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Common;
using Mediachase.Web.Console.Interfaces;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout.Framework.Apps.Order.Payments.Plugins.KlarnaCheckout
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        public string ValidationGroup { get; set; }

        public void LoadObject(object dto)
        {
            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
                return;

            var isProduction = bool.Parse(paymentMethod.GetParameter(KlarnaConstants.IsProduction, "false"));
            cbIsProduction.Checked = isProduction;
            txtMerchantId.Text = paymentMethod.GetParameter(KlarnaConstants.MerchantId, "");
            txtSecret.Text = paymentMethod.GetParameter(KlarnaConstants.Secret, "");

            ddlLocale.DataSource = Locale.Locales.OrderBy(c => c.Country)
                    .Select(c => new ListItem(c.Country, c.LocaleCode)).ToList();
            ddlLocale.DataBind();

            //Sweden set as default Locale
            ManagementHelper.SelectListItem(
                ddlLocale,
                paymentMethod.GetParameter(KlarnaConstants.Locale, "sv-se"),
                StringComparer.OrdinalIgnoreCase);
        }

        public void SaveChanges(object dto)
        {
            if (!Visible)
                return;

            var paymentMethod = dto as PaymentMethodDto;
            if (paymentMethod == null)
                return;

            paymentMethod.SetParameter(KlarnaConstants.IsProduction, cbIsProduction.Checked ? "true" : "false");
            paymentMethod.SetParameter(KlarnaConstants.MerchantId, txtMerchantId.Text);
            paymentMethod.SetParameter(KlarnaConstants.Secret, txtSecret.Text);

            paymentMethod.SetParameter(KlarnaConstants.Locale, ddlLocale.SelectedValue);
        }
    }
}
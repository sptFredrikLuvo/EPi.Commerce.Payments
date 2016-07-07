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
            txtMerchantId.Text = paymentMethod.GetParameter(NetaxeptConstants.MerchantIdField, string.Empty);
            txtToken.Text = paymentMethod.GetParameter(NetaxeptConstants.TokenField, string.Empty);

            var isProduction = bool.Parse(paymentMethod.GetParameter(NetaxeptConstants.IsProductionField, "false"));
            IsProductionCheckBox.Checked = isProduction;

            drdTerminalLanguage.SelectedValue = paymentMethod.GetParameter(NetaxeptConstants.TerminalLanguageField, GetDefaultLanguage(paymentMethod));
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
            paymentMethod.SetParameter(NetaxeptConstants.MerchantIdField, txtMerchantId.Text);
            paymentMethod.SetParameter(NetaxeptConstants.TokenField, txtToken.Text);
            paymentMethod.SetParameter(NetaxeptConstants.IsProductionField, (IsProductionCheckBox.Checked ? "true" : "false"));
            paymentMethod.SetParameter(NetaxeptConstants.TerminalLanguageField, drdTerminalLanguage.SelectedValue);
        }

        private string GetDefaultLanguage(PaymentMethodDto paymentMethod)
        {
            var currentPaymentLanguage = paymentMethod.PaymentMethodParameter[0].PaymentMethodRow.LanguageId;
            var defaultValue = string.Format("{0}_{1}", currentPaymentLanguage.ToLower(), currentPaymentLanguage.ToUpper());
            switch (currentPaymentLanguage)
            {
                case "no":
                    return "no_NO";
                case "sv":
                    return "sv_SE";
                case "da":
                    return "da_DK";
                case "de":
                    return "de_DE ";
                case "fi":
                    return "fi_FI";
                case "ru":
                    return "ru_RU";
                case "pl":
                    return "pl_PL";
                case "es":
                    return "es_ES";
                case "it":
                    return "it_IT";
                case "en":
                    return "en_GB";
                default:
                    return "en_GB";
            }
        }
    }
}
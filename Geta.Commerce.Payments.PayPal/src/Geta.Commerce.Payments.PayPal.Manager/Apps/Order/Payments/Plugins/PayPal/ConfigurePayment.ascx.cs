using System;
using System.Data;
using Geta.PayPal;
using Geta.PayPal.Extensions;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Web.Console.Interfaces;

namespace Geta.Commerce.Payments.PayPal.Manager.Apps.Order.Payments.Plugins.PayPal
{
    public partial class ConfigurePayment : System.Web.UI.UserControl, IGatewayControl
    {
        // Fields
        private PaymentMethodDto _paymentMethodDto;
        private string _validationGroup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurePayment"/> class.
        /// </summary>
        public ConfigurePayment()
        {
            this._validationGroup = string.Empty;
            this._paymentMethodDto = null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.BindData();
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        public void BindData()
        {
            if ((this._paymentMethodDto != null) && (this._paymentMethodDto.PaymentMethodParameter != null))
            {
                PaymentMethodDto.PaymentMethodParameterRow parameterByName = null;
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.UserParameter);
                if (parameterByName != null)
                {
                    this.APIUser.Text = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.PALParameter);
                if (parameterByName != null)
                {
                    this.PAL.Text = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.PasswordParameter);
                if (parameterByName != null)
                {
                    this.Password.Text = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.APISisnatureParameter);
                if (parameterByName != null)
                {
                    this.Signature.Text = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.SandBoxParameter);
                if (parameterByName != null)
                {
                    this.CheckBoxTest.Checked = parameterByName.Value == "1";
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.ExpChkoutURLParameter);
                if (parameterByName != null)
                {
                    this.ExpChkoutURL.Text = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.BusinessEmailParameter);
                if (parameterByName != null)
                {
                    this.BusinessEmail.Text = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.AllowChangeAddressParameter);
                if (parameterByName != null)
                {
                    this.CheckBoxAllowChangeAddress.Checked = parameterByName.Value == "1";
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.SkipConfirmPageParameter);
                if (parameterByName != null)
                {
                    this.CheckBoxSkipConfirmPage.Checked = parameterByName.Value == "1";
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.AllowGuestParameter);
                if (parameterByName != null)
                {
                    this.CheckBoxGuestCheckout.Checked = parameterByName.Value == "1";
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.PaymentActionParameter);
                if (parameterByName != null)
                {
                    this.DropDownListPaymentAction.SelectedValue = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.SuccessUrl);
                if (parameterByName != null)
                {
                    this.SuccessUrl.Text = parameterByName.Value;
                }
                parameterByName = this.GetParameterByName(PayPalConstants.Configuration.CancelUrl);
                if (parameterByName != null)
                {
                    this.CancelUrl.Text = parameterByName.Value;
                }
            }
            else
            {
                this.Visible = false;
            }
        }

        private PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(string name)
        {
            return this._paymentMethodDto.GetParameterByName(name);
        }

        /// <summary>
        /// Create parameters (settings) for paymentMethodDto (which is PayPal).
        /// <example>APIUsername, APIPassword, PayPalCheckoutUrl, ...</example>
        /// </summary>
        /// <param name="dto">the PayPal payment method</param>
        /// <param name="name">param's name</param>
        /// <param name="value">param's value</param>
        /// <param name="paymentMethodId">id of PayPal payment method</param>
        private void CreateParameter(PaymentMethodDto dto, string name, string value, Guid paymentMethodId)
        {
            PaymentMethodDto.PaymentMethodParameterRow row = dto.PaymentMethodParameter.NewPaymentMethodParameterRow();
            row.PaymentMethodId = paymentMethodId;
            row.Parameter = name;
            row.Value = value;
            if (row.RowState == DataRowState.Detached)
            {
                dto.PaymentMethodParameter.Rows.Add(row);
            }
        }


        #region IGatewayControl Members

        /// <summary>
        /// Loads the PaymentMethodDto object
        /// </summary>
        /// <param name="dto">The PaymentMethodDto object</param>
        public void LoadObject(object dto)
        {
            this._paymentMethodDto = dto as PaymentMethodDto;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="dto">The dto.</param>
        public void SaveChanges(object dto)
        {
            if (this.Visible)
            {
                this._paymentMethodDto = dto as PaymentMethodDto;
                if ((this._paymentMethodDto != null) && (this._paymentMethodDto.PaymentMethodParameter != null))
                {
                    Guid paymentMethodId = Guid.Empty;
                    if (this._paymentMethodDto.PaymentMethod.Count > 0)
                    {
                        paymentMethodId = this._paymentMethodDto.PaymentMethod[0].PaymentMethodId;
                    }

                    PaymentMethodDto.PaymentMethodParameterRow parameterByName = null;
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.UserParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.APIUser.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.UserParameter, this.APIUser.Text, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.BusinessEmailParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.BusinessEmail.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.BusinessEmailParameter, this.BusinessEmail.Text, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.PALParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.PAL.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.PALParameter, this.PAL.Text, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.PasswordParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.Password.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.PasswordParameter, this.Password.Text, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.APISisnatureParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.Signature.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.APISisnatureParameter, this.Signature.Text, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.ExpChkoutURLParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.ExpChkoutURL.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.ExpChkoutURLParameter, this.ExpChkoutURL.Text, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.SandBoxParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = (this.CheckBoxTest.Checked ? "1" : "0");
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.SandBoxParameter, (this.CheckBoxTest.Checked ? "1" : "0"), paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.AllowChangeAddressParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = (this.CheckBoxAllowChangeAddress.Checked ? "1" : "0");
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.AllowChangeAddressParameter, (this.CheckBoxAllowChangeAddress.Checked ? "1" : "0"), paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.SkipConfirmPageParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = (this.CheckBoxSkipConfirmPage.Checked ? "1" : "0");
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.SkipConfirmPageParameter, (this.CheckBoxSkipConfirmPage.Checked ? "1" : "0"), paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.AllowGuestParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = (this.CheckBoxGuestCheckout.Checked ? "1" : "0");
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.AllowGuestParameter, (this.CheckBoxGuestCheckout.Checked ? "1" : "0"), paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.PaymentActionParameter);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.DropDownListPaymentAction.SelectedValue;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.PaymentActionParameter, this.DropDownListPaymentAction.SelectedValue, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.SuccessUrl);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.SuccessUrl.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.SuccessUrl, this.SuccessUrl.Text, paymentMethodId);
                    }
                    parameterByName = this.GetParameterByName(PayPalConstants.Configuration.CancelUrl);
                    if (parameterByName != null)
                    {
                        parameterByName.Value = this.CancelUrl.Text;
                    }
                    else
                    {
                        this.CreateParameter(this._paymentMethodDto, PayPalConstants.Configuration.CancelUrl, this.CancelUrl.Text, paymentMethodId);
                    }
                }
            }

        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>The validation group.</value>
        public string ValidationGroup
        {
            get
            {
                return _validationGroup;
            }
            set
            {
                _validationGroup = value;
            }
        }

        #endregion
    }
}
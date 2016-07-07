<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Geta.EPi.Payments.Netaxept.CommerceManager.Apps.Order.Payments.Plugins.NetaxeptCheckout.ConfigurePayment" %>

<h2>Service setting</h2>

<table class="DataForm">
    <tbody>
         <tr>
            <td class="FormLabelCell">Merchant id:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtMerchantId" />
                <asp:RequiredFieldValidator ID="requiredMerchantId" runat="server" ControlToValidate="txtMerchantId" ErrorMessage="Merchant is required." />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Token:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtToken"  />
                <asp:RequiredFieldValidator ID="requiredToken" runat="server" ControlToValidate="txtToken" ErrorMessage="The token is required." />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Is production:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="IsProductionCheckBox" />
            </td>
        </tr>
         <tr>
            <td class="FormLabelCell">Terminal language:</td>
            <td class="FormFieldCell">
                <asp:DropDownList ID="drdTerminalLanguage" runat="server">
                    <Items>
                       <asp:ListItem Text="Norwegian" Value="no_NO" />
                       <asp:ListItem Text="Swedish" Value="sv_SE" />
                       <asp:ListItem Text="Danish" Value="da_DK" />
                       <asp:ListItem Text="German" Value="de_DE" />
                       <asp:ListItem Text="Finnish" Value="fi_FI" />
                       <asp:ListItem Text="Russian" Value="ru_RU" />
                       <asp:ListItem Text="Polish" Value="pl_PL" />
                       <asp:ListItem Text="Spanish" Value="es_ES" />
                       <asp:ListItem Text="Italian" Value="it_IT" />
                       <asp:ListItem Text="English" Value="en_GB" />
                    </Items>
                </asp:DropDownList>
            </td>
        </tr>
    </tbody>
</table>
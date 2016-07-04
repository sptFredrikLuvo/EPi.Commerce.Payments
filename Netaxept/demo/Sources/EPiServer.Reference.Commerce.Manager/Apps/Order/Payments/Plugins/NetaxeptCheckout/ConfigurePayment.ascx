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
       
    </tbody>
</table>
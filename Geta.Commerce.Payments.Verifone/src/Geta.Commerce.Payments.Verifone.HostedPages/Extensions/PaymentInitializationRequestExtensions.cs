using System.Globalization;
using System.Linq;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Geta.Commerce.Payments.Verifone.HostedPages.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Geta.Commerce.Payments.Verifone.HostedPages.Extensions
{
    public static class PaymentInitializationRequestExtensions
    {
        public static void InitializeFromOrder(this PaymentInitializationRequest request, PurchaseOrder purchaseOrder)
        {
            var currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
            var paymentService = ServiceLocator.Current.GetInstance<IVerifonePaymentService>();
            OrderAddress orderAddress = purchaseOrder.OrderAddresses.First();

            request.PaymentLocale = paymentService.GetPaymentLocale(ContentLanguage.PreferredCulture);
            request.OrderNumber = purchaseOrder.Id.ToString(CultureInfo.InvariantCulture.NumberFormat);
            request.OrderCurrencyCode = currentMarket.GetCurrentMarket().DefaultCurrency.CurrencyCode;
            request.OrderGrossAmount = purchaseOrder.Total.ToAmountString();
            request.OrderNetAmount = purchaseOrder.SubTotal.ToAmountString();
            //request.OrderVatAmount = purchaseOrder.TaxTotal.ToAmountString();
            request.BuyerFirstName = orderAddress.FirstName;
            request.BuyerLastName = orderAddress.LastName;
            request.BuyerPhoneNumber = orderAddress.DaytimePhoneNumber ?? orderAddress.EveningPhoneNumber;
            request.BuyerEmailAddress = orderAddress.Email;
            request.DeliveryAddressLineOne = orderAddress.Line1;
            request.DeliveryAddressLineTwo = orderAddress.Line2;
            request.DeliveryAddressPostalCode = orderAddress.PostalCode;
            request.DeliveryAddressCity = orderAddress.City;
        }
    }
}
using System;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using Geta.Netaxept.Checkout;


namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentMethodViewModelResolver
    {
        public static IPaymentMethodViewModel<PaymentMethodBase> Resolve(string paymentMethodName)
        {
            switch (paymentMethodName)
            {
                case "CashOnDelivery":
                    return new CashOnDeliveryViewModel { PaymentMethod = new CashOnDeliveryPaymentMethod() };
                case "GenericCreditCard":
                    return new GenericCreditCardViewModel { PaymentMethod = new GenericCreditCardPaymentMethod() };
                case "Authorize":
                    return new AuthorizeViewModel { PaymentMethod = new AuthorizePaymentMethod() };
                case "netaxept":
                    var model =  new NetaxeptViewModel { PaymentMethod = new NetaxeptPaymentMethod() };

                    var customerContextFacade = ServiceLocator.Current.GetInstance<CustomerContextFacade>();

                    model.CustomerCardMaskedFieldName = customerContextFacade.CurrentContact.CurrentContact[NetaxeptConstants.CustomerCardMaskedFieldName]?.ToString();
                    model.CustomerCardExpirationDateFieldName = customerContextFacade.CurrentContact.CurrentContact[NetaxeptConstants.CustomerCardExpirationDateFieldName]?.ToString();
                    model.CustomerCardExpirationDateFieldName = customerContextFacade.CurrentContact.CurrentContact[NetaxeptConstants.CustomerCardExpirationDateFieldName]?.ToString();
                    model.CustomerCardPaymentMethodField = customerContextFacade.CurrentContact.CurrentContact[NetaxeptConstants.CustomerCardPaymentMethodFieldName]?.ToString();
                    model.CustomerCardIssuerCountryField = customerContextFacade.CurrentContact.CurrentContact[NetaxeptConstants.CustomerCardIssuerCountryFieldName]?.ToString();
                    model.CustomerCardIssuerIdField = customerContextFacade.CurrentContact.CurrentContact[NetaxeptConstants.CustomerCardIssuerIdFieldName]?.ToString();
                    model.CustomerCardIssuerField = customerContextFacade.CurrentContact.CurrentContact[NetaxeptConstants.CustomerCardIssuerFieldName]?.ToString();

                    return model;
            }

            throw new ArgumentException("No view model has been implemented for the method " + paymentMethodName, "paymentMethodName");
        }
    }
}
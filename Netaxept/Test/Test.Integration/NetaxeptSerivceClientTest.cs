using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Geta.Netaxept.Checkout;
using Geta.Netaxept.Checkout.Models;
using Xunit;

namespace Test.Integration
{
    public class NetaxeptSerivceClientTest
    {
        [Fact]
        public void RegisterPayment()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var client = new NetaxeptServiceClient(new ClientConnection(appSettings["Netaxept:MerchantId"], appSettings["Netaxept:Token"], false));

            var paymentRequest = CreatePaymentRequest();
            var transactionId = client.Register(paymentRequest);

            Assert.NotEmpty(transactionId);
        }

        [Fact]
        public void QueryPayment()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var client = new NetaxeptServiceClient(new ClientConnection(appSettings["Netaxept:MerchantId"], appSettings["Netaxept:Token"], false));

            var paymentRequest = CreatePaymentRequest();
            var transactionId = client.Register(paymentRequest);

            Assert.NotEmpty(transactionId);

            var result = client.Query(transactionId);

            Assert.NotNull(result);
        }

        [Fact]
        public void RegisterPayment_Invalid_PhoneNumber()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var client = new NetaxeptServiceClient(new ClientConnection(appSettings["Netaxept:MerchantId"], appSettings["Netaxept:Token"], false));

            var paymentRequest = CreatePaymentRequest();
            paymentRequest.CustomerPhoneNumber = "1";

            Exception ex = Assert.Throws<ValidationException>(() => client.Register(paymentRequest));

            Assert.Equal("Invalid phonenumber, e.g. +4712345678, +469876543", ex.Message);
        }

        private PaymentRequest CreatePaymentRequest()
        {
            var request = new PaymentRequest();
            request.EnableEasyPayments = true;

            request.Amount = "100";
            request.CurrencyCode = "USD";
            request.OrderDescription = "Netaxept order";
            request.OrderNumber = "1234567890";
            
            request.Language = "en_GB";

            request.SuccessUrl = "http://www.google.nl";

            request.CustomerNumber = "1";
            request.CustomerFirstname = "Patrick";
            request.CustomerLastname = "van Kleef";
            request.CustomerEmail = "patrick@geta.no";
            request.CustomerAddress1 = "Kralingen";
            request.CustomerAddress2 = "118";
            request.CustomerPostcode = "1566CC";
            request.CustomerTown = "Assendelft";
            request.CustomerCountry = "NL";
            request.CustomerPhoneNumber = "+31626941208";
            return request;
        }
    }
}

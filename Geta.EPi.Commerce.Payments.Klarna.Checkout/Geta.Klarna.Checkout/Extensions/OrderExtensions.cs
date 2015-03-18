using System;
using Klarna.Checkout;
using Newtonsoft.Json.Linq;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class OrderExtensions
    {
        internal static string GetSnippet(this Order order)
        {
            var gui = order.GetValue("gui") as JObject;
            if (gui == null)
            {
                throw new Exception("Klarna gui can't be retrieved");
            }
            var snippet = gui["snippet"];
            return snippet.ToString();
        }

        internal static BillingAddress GetBillingAddress(this Order order)
        {
            var addressJObject = order.GetValue("billing_address") as JObject;
            if (addressJObject == null)
            {
                throw new Exception("Klarna billing_address can't be retrieved");
            }
            var address = new BillingAddress();
            PopulateAddress(addressJObject, address);
            return address;
        }

        internal static ShippingAddress GetShippingAddress(this Order order)
        {
            var addressJObject = order.GetValue("shipping_address") as JObject;
            if (addressJObject == null)
            {
                throw new Exception("Klarna shipping_address can't be retrieved");
            }
            var address = new ShippingAddress();
            PopulateAddress(addressJObject, address);
            return address;
        }

        private static void PopulateAddress(JObject addressJObject, Address address)
        {
            address.GivenName = addressJObject["given_name"].ToString();
            address.FamilyName = addressJObject["family_name"].ToString();
            address.StreetAddress = addressJObject["street_address"].ToString();
            address.StreetName = addressJObject["street_name"].ToString();
            address.StreetNumber = addressJObject["street_number"].ToString();
            address.PostalCode = addressJObject["postal_code"].ToString();
            address.City = addressJObject["city"].ToString();
            address.Country = addressJObject["country"].ToString();
            address.EmailAddress = addressJObject["email"].ToString();
            address.PhoneNumber = addressJObject["phone"].ToString();
            address.Title = addressJObject["title"].ToString();
        }
    }
}
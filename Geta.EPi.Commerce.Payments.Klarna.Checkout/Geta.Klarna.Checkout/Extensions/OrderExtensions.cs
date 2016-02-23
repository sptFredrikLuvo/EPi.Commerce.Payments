using System;
using System.Collections.Generic;
using Geta.Klarna.Checkout.Models;
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

       internal static string GetStringField(this Order order, string fieldName)
        {
            var status = order.GetValue(fieldName);
            if (status == null)
            {
                throw new Exception(string.Format("Klarna {0} can't be retrieved", fieldName));
            }
            return status.ToString();
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

        internal static void Confirm(this Order order, MerchantReference merchantReference)
        {
            if ((string)order.GetValue("status") != "checkout_complete") return;

            var data = new Dictionary<string, object>
            {
                {"status", "created"}
            };

            if (!merchantReference.IsEmpty)
            {
                data.Add("merchant_reference", merchantReference.ToDictionary());
            }

            order.Update(data);
        }

        private static void PopulateAddress(JObject addressJObject, Address address)
        {
            dynamic addr = addressJObject;

            address.GivenName = addr.given_name;
            address.FamilyName = addr.family_name;
            address.StreetAddress = addr.street_address;
            address.StreetName = addr.street_name;
            address.StreetNumber = addr.street_number;
            address.PostalCode = addr.postal_code;
            address.City = addr.city;
            address.Country = addr.country;
            address.EmailAddress = addr.email;
            address.PhoneNumber = addr.phone;
            address.Title = addr.title;
        }
    }
}
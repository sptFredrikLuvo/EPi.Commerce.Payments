using System.Collections.Generic;
using Geta.Klarna.Checkout.Models;

namespace Geta.Klarna.Checkout.Extensions
{
    internal static class AddressExtensions
    {
        internal static Dictionary<string, object> ToDictionary(this Address address)
        {
            return new Dictionary<string, object>
            {
                {"postal_code", address.PostalCode},
                {"email", address.EmailAddress}
            };
        }
    }
}

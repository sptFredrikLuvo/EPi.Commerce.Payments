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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Geta.Klarna.Checkout.Models
{
    /// <summary>
    /// This class can be used to parse json as order data - typically for order data sent as part as the Validate call
    /// </summary>
    public class OrderJsonWrapper
    {
        public string Id { get; set; }
        public string Status { get; set; }

        public ItemCart Cart { get; set; }
    }

    public class ItemCart
    {
        [JsonProperty(PropertyName = "total_price_including_tax")]
        public int TotalPriceIncTaxInCents { get; set; }

        public decimal TotalPriceIncTax { get { return ((decimal)TotalPriceIncTaxInCents) / 100; } }


        [JsonProperty(PropertyName = "total_price_excluding_tax")]
        public int TotalPriceExTaxInCents { get; set; }

        public decimal TotalPriceExTax { get {return ((decimal)TotalPriceExTaxInCents) / 100; } }
    }
}

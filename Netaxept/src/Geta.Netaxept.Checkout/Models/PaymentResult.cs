using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Netaxept.Checkout.Models
{
    public class PaymentResult
    {
        public string PanHash { get; set; }
        public bool Cancelled { get; set; }
        public bool ErrorOccurred { get; set; }
        public string ErrorMessage { get; set; }
    }
}

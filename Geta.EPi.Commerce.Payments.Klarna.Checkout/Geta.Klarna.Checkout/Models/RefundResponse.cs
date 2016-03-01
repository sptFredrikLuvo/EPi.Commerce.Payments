using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Klarna.Checkout.Models
{
    public class RefundResponse : IResult
    {
        public string InvoiceNumber { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
    }
}

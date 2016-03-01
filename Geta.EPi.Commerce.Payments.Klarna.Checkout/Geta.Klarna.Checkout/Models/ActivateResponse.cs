using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Klarna.Api;

namespace Geta.Klarna.Checkout.Models
{
    public class ActivateResponse : IResult
    {
        public ActivateResponse(string invoiceNumber, RiskStatus status, string transactionId)
        {
            InvoiceNumber = invoiceNumber;
            RiskStatus = status;
            TransactionId = transactionId;
        }

        public RiskStatus RiskStatus { get; private set; }
        public string TransactionId { get; private set; }
        public string InvoiceNumber { get; private set; }

        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
    }

   
}

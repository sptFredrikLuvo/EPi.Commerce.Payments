
namespace Geta.Netaxept.Checkout.Models
{
    public class ProcessResult
    {
        public bool ErrorOccurred { get; set; }
        public string ErrorMessage { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string ResponseSource { get; set; }
    }
}

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business.PaymentSteps
{
    public class PaymentStepResult
    {
        public bool IsSuccessful { get; set; }
        public string RedirectUrl { get; set; }
        public string Message { get; set; }
    }
}
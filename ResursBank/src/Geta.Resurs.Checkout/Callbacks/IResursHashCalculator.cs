namespace Geta.Resurs.Checkout.Callbacks
{
    public interface IResursHashCalculator
    {
        string Compute(CallbackData parameters, string salt);
    }
}
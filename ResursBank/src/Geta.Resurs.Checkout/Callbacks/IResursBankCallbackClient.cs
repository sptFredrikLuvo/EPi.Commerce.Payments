namespace Geta.Resurs.Checkout.Callbacks
{
    public interface IResursBankCallbackClient
    {
        void RegisterCallbackUrl(CallbackEventType callbackEventType, string url);
        void UnRegisterCallbackUrl(CallbackEventType callbackEventType);
        bool ProcessCallback(CallbackData callbackData, string digest);
        bool CheckDigest(CallbackData callbackData, string digest);
    }
}
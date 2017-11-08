using Geta.Resurs.Checkout.Model;

namespace Geta.Resurs.Checkout.Callbacks
{
    public interface IResursBankCallbackClientFactory
    {
        IResursBankCallbackClient Init();
        IResursBankCallbackClient Init(ResursCredential resursCredential);
    }
}
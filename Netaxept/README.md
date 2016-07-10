Library for Netaxept (EasyPayments) Checkout integration with EPiServer Commerce
=============

![](http://tc.geta.no/app/rest/builds/buildType:(id:TeamFrederik_EPiCommercePaymentsKlarnaCheckout_Debug)/statusIcon)

## What is Geta.EPi.Commerce.Payments.Netaxept.Checkout?

Geta.EPi.Commerce.Payments.Netaxept.Checkout is library which helps to integrate Netaxept Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of three assemblies: 
* Geta.Netaxept.Checkout is wrapper for Netaxept Checkout API and simplifies API usage 
* Geta.EPi.Commerce.Payments.Netaxept.Checkout contains extensions and helpers for easier EPiServer Commerce and Klarna Checkout integration 
* Geta.EPi.Payments.Netaxept.CommerceManager contains .ascx for payment method settings for Commerce Manager

## How to get started?

Start by installing NuGet packages (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Geta.EPi.Commerce.Payments.Netaxept.Checkout

For the Commerce Manager site run the following package:

    Install-Package Geta.EPi.Payments.Netaxept.CommerceManager

## Setup

### Configure Commerce Manager

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name**
- **System Keyword** - use some Keyword which you can use later to find this payment method in your code
- **Language**
- **Class Name** - choose **Geta.EPi.Commerce.Payments.Netaxept.Checkout.NetaxeptCheckoutPaymentGateway**
- **Payment Class** - choose **Mediachase.Commerce.Orders.OtherPayment**
- **IsActive** - **Yes**
- select shipping methods available for this payment
- navigate to parameters tab and fill inn settings (see screenshot below)

![Payment method settings](/Netaxept/screenshots/overview.PNG?raw=true "Payment method settings")

![Payment method settings](/Netaxept/screenshots/parameters.PNG?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

In **Markets** tab select market for which this payment will be available.

### Payment process

Currently the Nuget package only supports payment by (credit)card. 
- **Register**  - Before the user is redirected first the payment must be registered at Netaxept. This call will return a transaction id, we will need it for the next steps.
- **Terminal**  - Immediately after registering the payment, the user is redirected to the terminal of Netaxept. Here the user can select a payment option and enter his card information.
- **Auth**      - User is redirected back to the website. When we receive an ok status (meaning the payment has pass the validation), we can call the Auth method to reserve the amount.
- **Capture**   - When the order is ready for shipment, we can call the Capture method.
- **Credit**    - The Credit method can be used for returns.

More information about payment flows:
https://shop.nets.eu/web/partners/flow-outline

### Callback

The user will be redirected back to the website after fill out the card information in the terminal. The URL needs to be set on the PaymentGateway before running the checkout workflow (OrderGroupWorkflowManager.CartCheckOutWorkflowName).
Below an example. 

```
var netaxept = checkoutViewModel.Payment as NetaxeptViewModel;
if (netaxept != null)
{
    var netaxeptPaymentMethod = checkoutViewModel.Payment.PaymentMethod as NetaxeptCheckoutPaymentGateway;
    if (netaxeptPaymentMethod != null)
    {
        netaxeptPaymentMethod.CallbackUrl = "http://" + this.Request.Url.DnsSafeHost + Url.Action("Index", "PaymentCallback");
    }
}

```

The transaction id is passed as parameter to the Index method. On the NetaxeptCheckoutPaymentGateway the ProcessAuthorization method should be called. This method will complete the payment. The return value indicate if the payment is successfull otherwise
an error code is returned. If the result is ok, then the cart should be saved as a purchase order to finalize the checkout. During the payment process, notes are saved on the cart (register and auth steps), you can copy the notes to the purchase order if you 
would like a complete history of the payment process.

![Payment method settings](/Netaxept/screenshots/notes.PNG?raw=true "Order notes")

```
public class PaymentCallbackController : Controller
{
    private ICheckoutService _checkoutService;
    private CustomerContextFacade _customerContext;

    public RedirectResult Index(string transactionId)
    {
        _checkoutService = ServiceLocator.Current.GetInstance<ICheckoutService>();
        _customerContext = ServiceLocator.Current.GetInstance<CustomerContextFacade>();

        PurchaseOrder purchaseOrder = null;

        Mediachase.Commerce.Orders.Cart cart = new CartHelper(Mediachase.Commerce.Orders.Cart.DefaultName).Cart;
        
        var payment = GetPayment(cart);

        var netaxeptCheckoutPaymentGateway = new NetaxeptCheckoutPaymentGateway();

        var result = netaxeptCheckoutPaymentGateway.ProcessAuthorization(payment, transactionId);
        if (result.Result == PaymentResponseCode.Success)
        {
            purchaseOrder = _checkoutService.SaveCartAsPurchaseOrder();

            // this will copy all notes from the Cart to the PurchaseOrder
            CopyNotesFromCartToPurchaseOrder(purchaseOrder, cart); 

            _checkoutService.DeleteCart();

            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
            };

            return new RedirectResult(new UrlBuilder("/checkout/order-confirmation/") { QueryCollection = queryCollection }.ToString());
        }
        return new RedirectResult(new UrlBuilder("/error-pages/payment-failed/").ToString());
    }

    /// <summary>
    /// Copy notes from cart to purchse order
    /// </summary>
    /// <param name="purchaseOrder"></param>
    /// <param name="cart"></param>
    private void CopyNotesFromCartToPurchaseOrder(PurchaseOrder purchaseOrder, Mediachase.Commerce.Orders.Cart cart)
    {
        foreach (var note in cart.OrderNotes.OrderByDescending(n => n.Created))
        {
            OrderNote on = purchaseOrder.OrderNotes.AddNew();
            on.Detail = note.Detail;
            on.Title = note.Title;
            on.Type = OrderNoteTypes.System.ToString();
            on.Created = note.Created;
            on.CustomerId = note.CustomerId;
        }
        purchaseOrder.AcceptChanges();
    }

    /// <summary>
    /// Get payment
    /// </summary>
    /// <param name="cart"></param>
    /// <returns></returns>
    private Mediachase.Commerce.Orders.Payment GetPayment(Mediachase.Commerce.Orders.Cart cart)
    {
        if (cart.OrderForms == null || cart.OrderForms.Count == 0 || cart.OrderForms[0].Payments == null || cart.OrderForms[0].Payments.Count == 0)
            return null;

        List<Mediachase.Commerce.Orders.Payment> payments = cart.OrderForms[0].Payments.Where(p => p.Status != PaymentStatus.Failed.ToString()).ToList();
        payments = PaymentTransactionTypeManager.GetResultingPaymentsByTransactionType(payments, TransactionType.Authorization).ToList();

        if (payments.Any())
            return payments.First();
        return null;
    }
}
```


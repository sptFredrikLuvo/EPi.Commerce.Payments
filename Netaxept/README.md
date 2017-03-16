Library for Netaxept (EasyPayments) Checkout integration with EPiServer Commerce
=============

![](http://tc.geta.no/app/rest/builds/buildType:(id:TeamFrederik_EPiCommercePaymentsKlarnaCheckout_Debug)/statusIcon)

## What is Geta.EPi.Commerce.Payments.Netaxept.Checkout?

Geta.EPi.Commerce.Payments.Netaxept.Checkout is a library which helps to integrate Netaxept Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of three assemblies: 
* Geta.Netaxept.Checkout is wrapper for Netaxept Checkout API and simplifies API usage 
* Geta.EPi.Commerce.Payments.Netaxept.Checkout contains extensions and helpers for easier EPiServer Commerce and Klarna Checkout integration 
* Geta.EPi.Payments.Netaxept.CommerceManager contains .ascx for payment method settings for Commerce Manager

### Payment process

Currently, the Nuget package only supports payment by (credit)card. Below a description of the supported payment process.
- **Register**  - Before the user is redirected first the payment must be registered at Netaxept. This call will return a transaction id, we will need it for the next steps.
- **Terminal**  - Immediately after registering the payment, the user is redirected to the terminal of Netaxept. Here the user can select a payment option and enter his card information.
- **Auth**      - User is redirected back to the website. When we receive an ok status (meaning the payment has passed the validation), we can call the Auth method to reserve the amount.
- **Capture**   - When the order is ready for shipment, we can call the Capture method.
- **Credit**    - The Credit method can be used for returns.

More information about payment flows:
https://shop.nets.eu/web/partners/flow-outline

### Easy payments ###

This package supports the easy payments functionality of Netaxept. This makes it possible to save the card information of the user at Netaxept. Whenever the user pays again with Netaxept only the CVC code needs to be entered.
After a payment, the panHash is saved on the (EPiServer Commerce) customer contact object. The next payment the panHash is sent to Netaxept for using the easy payments functionality.  

More information about easy payments:
https://shop.nets.eu/web/partners/home

## How to get started?

:grey_exclamation: **From version 0.1.18 the module is using the new Episerver order api with a dependecy to Episerver.Commerce.Core v10.2.3**

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
- navigate to parameters tab and fill in settings (see screenshot below)

![Payment method settings](/Netaxept/docs/screenshots/overview.PNG?raw=true "Payment method settings")

The merchant id and token are necessary for establishing a connection to the service of Netaxept.

![Payment method settings](/Netaxept/docs/screenshots/parameters.PNG?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

In the **Markets** tab select a market for which this payment will be available.

### Creating the payment object

The user will be redirected back to the website after filling out the card information in the terminal. The URL needs to be set on the PaymentGateway before processing payment (no more workflows).
Below an example. 

```
  var urlHelper = new UrlHelper(System.Web.HttpContext.Current.Request.RequestContext);
  var netaxeptPaymentCallbackUrl = "http://" + System.Web.HttpContext.Current.Request.Url.DnsSafeHost +
                                             urlHelper.Action("Index", "PaymentCallback");
            
  var currency = _currencyService.GetCurrentCurrency();
  var amount = orderForm.GetTotal(_currentMarket.GetCurrentMarket(), currency).Amount;
  // creating the payment object
  var payment = paymentViewModel.PaymentMethod.CreatePayment(amount, cart);
  payment.Properties[NetaxeptConstants.CallbackUrl] = netaxeptPaymentCallbackUrl;

  orderForm.Payments.Clear();
  orderForm.Payments.Add(payment);
  // billing address is now part of payment object          
  var address = _orderAddressService.CreateOrUpdateBillingAddressFromModel(cart, payment, checkoutPageView.BillingAddress);
  payment.BillingAddress = address;

  _orderRepository.Save(cart);
  
   //Process payments for the cart
   cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);

```

The transaction id is passed as parameter to the Index method. On the NetaxeptCheckoutPaymentGateway the ProcessAuthorization method should be called. This method will complete the payment. The return value indicates if the payment is successfully otherwise
an error code is returned. If the result is ok, then the cart should be saved as a purchase order to finalize the checkout. During the payment process, notes are saved on the cart (register and auth steps), you can copy the notes to the purchase order if you 
would like a complete history of the payment process.

![Order notes](/Netaxept/docs/screenshots/notes.PNG?raw=true "Order notes")

```
public PaymentCallbackController(IOrderGroupCalculator orderGroupCalculator,
            IOrderRepository orderRepository,
            IOrderGroupFactory orderGroupFactory,
            IContentRepository icontentRepository, 
            CustomerContextFacade customerContext, 
            IContentRepository contentRepository, 
            ICartService cartService, 
            IOrderService orderService)
        {
            _orderGroupCalculator = orderGroupCalculator;
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _customerContext = customerContext;
            _contentRepository = contentRepository;
            _cartService = cartService;
        }
        
        
    public RedirectResult Index(string transactionId)
    {
       var checkoutPage = _contentRepository.GetFirstChild<CheckoutPage>(ContentReference.StartPage);
       var responseCode = Request.QueryString["responseCode"] as string;

       if (responseCode == "Cancel")
       {
        _log.Log(Level.Debug, "Payment cancelled by user. Redirecting back to checkout.");
         return RedirectBackToCheckout(checkoutPage, _paymentRedirect.GetCancelReasonCode());
       }

       IPurchaseOrder purchaseOrder = null;

       var cart = _cartService.LoadCart(_cartService.DefaultCartName);
       if (cart != null)
       // order already processed or something failed - show friendly error
       {
            var payment = GetPaymentByStatus(cart, PaymentStatus.Pending, TransactionType.Authorization);

            var orderTotal = cart.GetTotal(_orderGroupCalculator).Amount;

            // Security check: Compare cart value with payment to be processed. Check against tempering with shopping cart
            if (payment.Amount != orderTotal)
            {
               // payment failed - log error
               var message =
                        $"Wrong amount! First OrderForm.Total={orderTotal}. Payment amount is {payment.Amount}";
                _log.Log(Level.Warning, message);
                 return RedirectBackToCheckout(checkoutPage, _paymentRedirect.GetInvalidAmountCode());
            }

           var netaxeptCheckoutPaymentGateway = new NetaxeptCheckoutPaymentGateway();
           var result = netaxeptCheckoutPaymentGateway.ProcessAuthorization(
                    payment,
                    cart.GetFirstForm(), 
                    cart, 
                    transactionId);
                if (result.Result == PaymentResponseCode.Success)
                {
                    payment.Status = PaymentStatus.Processed.ToString();
                    var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
                    purchaseOrder = _orderRepository.Load(orderReference) as IPurchaseOrder;

                    // Make sure ordernumber is same as generated with NETS
                    string orderNumber = cart.Properties[NetaxeptConstants.CartOrderNumberTempField] as string;
                    if (!string.IsNullOrEmpty(orderNumber))
                        purchaseOrder.OrderNumber = orderNumber;

                    // this will copy all notes from the Cart to the PurchaseOrder
                    CopyNotesFromCartToPurchaseOrder(purchaseOrder, cart);

                    string currentContactId = string.Empty;
                    //if not a logged in user, we see if there is a contact with email address, else we create one
                    if (!User.Identity.IsAuthenticated)
                    {
                        currentContactId = _orderService.SetOrCreateCustomerToOrder(purchaseOrder, payment, string.Empty,
                            false);
                    }
                    else
                    {
                        currentContactId = _customerContext.CurrentContactId.ToString();
                    }

                    if(CustomerContext.Current.CurrentContact != null)
                        ((PurchaseOrder)purchaseOrder).CustomerName = CustomerContext.Current.CurrentContact.FullName;
                    
                    _orderRepository.Save(purchaseOrder);
                    _orderRepository.Delete(cart.OrderLink);

                    var queryCollection = new NameValueCollection
                    {
                        {"contactId", currentContactId},
                        {"orderNumber", purchaseOrder.OrderNumber.ToString(CultureInfo.InvariantCulture)}
                    };
                    
                    
                    var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(checkoutPage.ContentLink);
                    var orderConfirmationLink = new UrlBuilder(confirmationPage.ContentLink.GetFriendlyUrl(true)) { QueryCollection = queryCollection }.ToString();

                    return Redirect(orderConfirmationLink);
                }
                else
                {
                    // payment failed - log error
                    _log.Log(Level.Debug, $"Payment failed with message: {result.ErrorMessage}");
                    return RedirectBackToCheckout(checkoutPage, _paymentRedirect.GetErrorReasonCode());

                }
         }
         
          return new RedirectResult(new UrlBuilder("/error-pages/payment-failed/").ToString());
    }

    /// <summary>
    /// Copy notes from cart to purchse order
    /// </summary>
    /// <param name="purchaseOrder"></param>
    /// <param name="cart"></param>
    private void CopyNotesFromCartToPurchaseOrder(IPurchaseOrder purchaseOrder, ICart cart)
        {
            if (cart.Notes.Any())
            {
                foreach (var note in cart.Notes.OrderByDescending(n => n.Created))
                {
                    var on = _orderGroupFactory.CreateOrderNote(purchaseOrder);
                    on.Detail = note.Detail;
                    on.Title = note.Title;
                    on.Type = OrderNoteTypes.System.ToString();
                    on.Created = note.Created;
                    on.CustomerId = note.CustomerId;
                    purchaseOrder.Notes.Add(on);
                }

                // save changes
                _orderRepository.Save(purchaseOrder);
            }
        }

    private IPayment GetPaymentByStatus(ICart cart, PaymentStatus paymentStatus, TransactionType transactionType)
        {
            var orderForm = cart.GetFirstForm();
            var lineItems = cart.GetAllLineItems().ToList();

            if (orderForm == null || lineItems.Count == 0 || orderForm.Payments.Count == 0)
                return null;

            List<Payment> payments = orderForm.Payments.Where(p => p.Status == paymentStatus.ToString()).Cast<Payment>().ToList();
            payments = PaymentTransactionTypeManager.GetResultingPaymentsByTransactionType(payments, transactionType).ToList();

            return payments.FirstOrDefault();
        }
}
```

### Payment information

The card information of the payment is saved on payment object. This information can be displayed on the order confirmation page.

```
<p>
    Card information payment method: <strong>@Model.GetString(NetaxeptConstants.CardInformationPaymentMethodField)</strong> <br/>
    Card information expiry date: <strong>@Model.GetString(NetaxeptConstants.CardInformationExpiryDateField)</strong> <br />
    Card information issuer id: <strong>@Model.GetString(NetaxeptConstants.CardInformationIssuerIdField)</strong> <br />
    Card information issuer: <strong>@Model.GetString(NetaxeptConstants.CardInformationIssuerField)</strong> <br />
    Card information issuer country: <strong>@Model.GetString(NetaxeptConstants.CardInformationIssuerCountryField)</strong> <br />
    Card information masked pan: <strong>@Model.GetString(NetaxeptConstants.CardInformationMaskedPanField)</strong> <br />
</p>
```


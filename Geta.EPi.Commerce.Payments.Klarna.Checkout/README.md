Library for Klarna Checkout integration with EPiServer Commerce
=============

## What is Geta.EPi.Commerce.Payments.Klarna.Checkout?

Geta.EPi.Commerce.Payments.Klarna.Checkout is library which helps to integrate Klarna Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies - Geta.EPi.Commerce.Payments.Klarna.Checkout and Geta.Klarna.Checkout. Geta.Klarna.Checkout is wrapper for Klarna Checkout API and simplifies API usage. Geta.EPi.Commerce.Payments.Klarna.Checkout contains extensions and helpers for easier EPiServer Commerce and Klarna Checkout integration.

## How to get started?

Start by installing NuGet package (use [NuGet](http://nuget.episerver.com/)):

    Install-Package Geta.EPi.Commerce.Payments.Klarna.Checkout

## Setup

### Endpoints

Klarna Checkout requires four endpoints for checkout:
- Checkout where you have to call Klarna Checkout and provide all Klarna's cart items including shipping information, locale of Klarna Checkout, URL's of all endponts mentioned here. Klarna API will return HTML snippet which you have to render on your page where user will fill all required details for Klarna. Here is an example:

```
public ActionResult KlarnaCheckout()
{
    var cart = GetCart();
    var cartItems = cart
        .GetAllLineItems()
        .Select(item => item.ToCartItem())
        .ToList();

    var shipment = cart.OrderForms[0].Shipments.FirstOrDefault();
    if (shipment == null)
    {
        throw new Exception("Shipment not selected. Shippment should be persisted into the cart before checkout.");
    }

    cartItems.Add(shipment.ToCartItem());

    
	var baseUri = GetBaseUri();
    var currentCheckoutPageUrl = string.Format("{0}{1}", baseUri, currentPage.PublicUrl());

    var checkoutUris = new CheckoutUris(
                new Uri(currentCheckoutPageUrl),
                new Uri(currentCheckoutPageUrl + "KlarnaConfirm"),
                new Uri(currentCheckoutPageUrl + "KlarnaPush"),
                new Uri(currentCheckoutPageUrl + "KlarnaTerms"));

	//Note: _checkoutClient variable should be created as private variable or property using Provider settings
	
	//var providerSettings = PaymentHelper.GetProviderSettings(currentMarket, currentLanguageBranch);
	//var _checkoutClient = new CheckoutClient(providerSettings.OrderBaseUri, 
					//providerSettings.MerchantId, 
					//providerSettings.Secret);
	
	var response = _checkoutClient.Checkout(cartItems, PaymentSettings.CurrentLocale, checkoutUris);

    var model = new KlarnaCheckoutView(CurrentPage)
    {
        Snippet = response.Snippet
		KlarnaTransactionId = response.TransactionId;
    };

    if (ControllerContext.IsChildAction)
    {
        return PartialView(model);
    }
    return View(model);
}
```

- Confirm which is called after user confirmed the payment in first step. In this step you call Klarna Confirm, in response you will receive another HTML snippet to render on the page. This snippet contains information about payment confirmation. Example:

```
public ActionResult KlarnaConfirm(string klarnaOrder)
{
    var model = new KlarnaCheckoutViewModel(currentPage);
    model.KlarnaTransactionId = klarnaOrder;

	if (string.IsNullOrEmpty(klarnaOrder))
		return RedirectToAction("Index");

	try
	{
		// here you should rename cart (use transaction id) and remove anonymous cookie
		model.KlarnaHtmlSnippet = _checkoutClient.GetConfirm(klarnaOrder);

		return View(model);
	}
	catch (Exception ex)
	{
		_log.ErrorFormat("Klarna confirm failed. ", ex);

	}
	return View(model);
}
```

- Push is called from Klarna when order is confirmed, but status not updated to created. Klarna client's Confirm already sets status to created, but this is still required for Klarna Checkout. 
In this step you should create the order in your system. Here is an example:

```
public ActionResult KlarnaPush(string klarnaOrder)
{
    var response = CheckoutClient.Confirm(klarnaOrder);

	// Order updated from status complete to created
	if (response.Status == Geta.Klarna.Checkout.Models.OrderStatus.Created)
	{
		// Create order in your system. Response object contains billing address and more
		CreateOrder(response);
	}

    return new HttpStatusCodeResult(HttpStatusCode.OK);
}
```

- Terms is needed to display terms of your site in Klarna Checkout. It can be some MVC view or even static HTML file.

### Snippet rendering

Snippet is just string which contains HTML. To render it just call *@Html.Raw(Model.Snippet)*

### Configure Commerce Manager

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name**
- **System Keyword** - use some Keyword which you can use later to find this payment method in your code
- **Language**
- **Class Name** - choose **Geta.EPi.Commerce.Payments.Klarna.Checkout.KlarnaCheckoutPaymentGateway**
- **Payment Class** - choose **Mediachase.Commerce.Orders.OtherPayment**
- **IsActive** - **Yes**
- select shipping methods available for this payment

![Payment method settings](../screenshots/klarnaSettings.png?raw=true "Payment method settings")

In **Markets** tab select market for which this payment will be available.

## Creating NuGet package

Project contains _pack.bat_ file for easier packaging. It calls _nuget.exe_ (assuming it's in your environment PATH) with _-IncludeReferencedProjects_ to include referenced Geta.Klarna.Checkout assembly. You also can provide output directory as a parameter for _pack.bat_.

## More info

### Klarna Checkout API reference

https://developers.klarna.com/en/api-references-v1/klarna-checkout

### Klarna Checkout documentation

https://developers.klarna.com/en/klarna-checkout

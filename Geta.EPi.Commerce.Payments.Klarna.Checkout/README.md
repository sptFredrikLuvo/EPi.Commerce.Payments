Library for Klarna Checkout integration with EPiServer Commerce
=============

## What is Geta.EPi.Commerce.Payments.Klarna.Checkout?

Geta.EPi.Commerce.Payments.Klarna.Checkout is library which helps to integrate Klarna Checkout as one of the payment options in your EPiServer Commerce sites.
This library consists of two assemblies - Geta.EPi.Commerce.Payments.Klarna.Checkout and Geta.Klarna.Checkout. Geta.Klarna.Checkout is wrapper for Klarna Checkout API and simplifies API usage. Geta.EPi.Commerce.Payments.Klarna.Checkout contains extensions and helpers for easier EPiServer Commerce and Klarna Checkout integration.

## How to get started?

Start by installing NuGet package (use [Geta NuGet](http://nuget.geta.no/)):

    Install-Package Geta.EPi.Commerce.Payments.Klarna.Checkout

## Setup

### Endpoints

Klarna Checkout requires four endpoints for checkout:
- Checkout where you have to call Klarna Checkout and provide all Klarna's cart items including shipping information, locale of Klarna Checkout, URL's of all endponts mentioned here. Klarna API will return HTML snippet which you have to render on your page where user will fill all required details for Klarna. Here is an example:

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

        var checkoutUris = new CheckoutUris(
            GetActionFullUri("KlarnaCheckout"),
            GetActionFullUri("KlarnaConfirm"),
            GetActionFullUri("KlarnaPush"),
            GetActionFullUri("KlarnaTerms"));
        var response = _checkoutClient.Checkout(cartItems, Locale.Norway, checkoutUris);

        var model = new KlarnaCheckoutView(CurrentPage)
        {
            Snippet = response.Snippet
        };

        if (ControllerContext.IsChildAction)
        {
            return PartialView(model);
        }
        return View(model);
    }

- Confirm which is called after user confirmed the payment in first step. In this step you have to create order in your system and call Klarna Confirm. In response you will receive another HTML snippet to render on the page. This snippet contains information about payment confirmation. Example:

    public ActionResult KlarnaConfirm(string klarnaOrder)
    {
        var location = new Uri(klarnaOrder);

        var allPayments = PaymentManager.GetPaymentMethods(SiteContext.Current.LanguageName).PaymentMethod;
        var checkoutMethod = allPayments.FirstOrDefault(x => x.SystemKeyword.Equals("KlarnaCheckout"));
        if (checkoutMethod != null)
        {
            var method = new KlarnaCheckoutMethod
            {
                Name = checkoutMethod.Name,
                CommercePaymentMethodId = checkoutMethod.PaymentMethodId.ToString()
            };

            ConfirmCart(method);
        }

        var response = _checkoutClient.Confirm(location);

        var model = new KlarnaCheckoutView(CurrentPage)
        {
            Snippet = response.Snippet
        };
        if (ControllerContext.IsChildAction)
        {
            return PartialView(model);
        }
        return View(model);
    }

- Push is called from Klarna when order is confirmed, but status not updated to created. Klarna client's Confirm already sets status to created, but this is still required for Klarna Checkout. Here is an example:

    public ActionResult KlarnaPush(string klarnaOrder)
    {
        var location = new Uri(klarnaOrder);
        _checkoutClient.Acknowledge(location);

        return new HttpStatusCodeResult(HttpStatusCode.OK);
    }

- Terms is needed to display terms of your site in Klarna Checkout. It can be some MVC view or even static HTML file.

### Snippet rendering

Snippet is just string which contains HTML. To render it just call *@Html.Raw(Model.Snippet)*

### Configuration

Klarna Checkout client requires three parametrs to be configured. The easiest way to do it is to add them into _appSettings_ section of _Web.config_:

    <add key="KlarnaCheckout:MerchantId" value="1234" />
    <add key="KlarnaCheckout:SharedSecret" value="mySharedSecret" />
    <add key="KlarnaCheckout:OrderBaseUrl" value="https://checkout.testdrive.klarna.com/checkout/orders" />

First parameter is your Merchant Id and second is your shared secret which you get when registering in Klarna. Last one is order base URL - it s different for test and production environments. So be sure to change it appropriately for both environments.

Next step is configuring Klarna Checkout klient for use with EPiServer dependency injection:

    For<ICheckoutClient>().Use<CheckoutClient>()
        .Ctor<Uri>("orderBaseUri").EqualToAppSetting("KlarnaCheckout:OrderBaseUrl")
        .Ctor<string>("merchantId").EqualToAppSetting("KlarnaCheckout:MerchantId")
        .Ctor<string>("sharedSecret").EqualToAppSetting("KlarnaCheckout:SharedSecret");

## Creating NuGet package

Project contains _pack.bat_ file for easier packaging. It calls _nuget.exe_ (assuming it's in your environment PATH) with _-IncludeReferencedProjects_ to include referenced Geta.Klarna.Checkout assembly. You also can provide output directory as a parameter for _pack.bat_.
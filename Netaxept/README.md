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

### Endpoints

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


![Payment method settings](/Netaxept/screenshots/overview.png?raw=true "Payment method settings")

![Payment method settings](/Netaxept/screenshots/parameters.png?raw=true "Payment method parameters")

**Note: If the parameters tab is empty (or gateway class is missing), make sure you have installed the commerce manager nuget (see above)**

In **Markets** tab select market for which this payment will be available.


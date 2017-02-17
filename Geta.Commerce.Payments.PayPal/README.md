PayPal payment provider for EPiServer Commerce
=============


## What is Geta.EPi.Commerce.Payments.PayPal?

Geta.EPi.Commerce.Payments.PayPal is based on Episervers own payment provider for PayPal which is downloadable for older versions of Episerver Commerce. This implementation is upgraded to 10.2.0 and the gateway is converted to IPaymentPlugin and using abstractions. 

It consits of:

* Geta.Commerce.Payments.PayPal
* Geta.Commerce.Payments.PayPal.Manager
* Geta.PayPal


## Setup


### Solution specific
The solution needs to implement the following...


### Commerce Manager

Login into Commerce Manager and open **Administration -> Order System -> Payments**. Then click **New** and in **Overview** tab fill:

- **Name**
- **System Keyword** -  The System keyword must be "PayPal" 
- **Language**
- **Class Name** - choose **Geta.Commerce.Payments.PayPal.PayPalPaymentGateway**
- **Payment Class** - choose **Geta.Commerce.Payments.PayPal.PayPalPayment**
- **IsActive** - **Yes**
- Select shipping methods available for this payment
- Navigate to parameters tab and fill settings 

### Parameters tab (API settings)
- **Business email** - 
- **API Username** - 
- **Password** -
- **API Signature** - 
- **Use test environment (sandbox)** -
- **Allow buyers to change shipping address at PayPal** -
- **Payment action** -
- **Allow guest checkout** -
- **Express Checkout Url** -
- **Success URL** -
- **Cancel URL** -
- **Skip OrderConfirmation page** -
- **PayPal Secure Merchant Account ID (PAL) (optional)** -


**Note: If the parameters tab is empty, make sure you have installed the commerce manager nuget**

In **Markets** tab select market for which this payment will be available.
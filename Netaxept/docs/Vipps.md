Support for VIPPS 
=============

Vipps is a mobile payment solution launched by DNB for online stores. With Vipps through Netaxept you can allow your customers to pay their purchases with their payment cards via Vipps app in your online store.

For more details about business features and restrictions please see [VIPPS Get started guide](netaxept_vipps_getstartedguide_en_v0.5.docx)

## Known issue

During the approval process, Netaxept authorizes the payment automatically. There is a known error in the VIPPS application (iPhone related) that sometimes prevents the the callback url to be called.
If this happens the amount will be autorized but the order never created since this is part of the  PaymentCallbackController.

As a work around NETS provides a the possibility to define a custom callback url through the admin ui. See https://shop.nets.eu/web/partners/callback

When Netaxept sends a callback to the configured callback URL the transactionID is sent as a json in the request body. 

### How to solve it
Add a new api controller that acts as an endpoint for the callback. 

The controller should pick up the transaction id from json body and 
 - Query NETS to check if authorize has been approved
 - Use order search (and payment meta fields) to find any carts that has not been processed
 - If match: process cart (create order) and send email confirmation
 
 Example: https://github.com/Geta/sport1/blob/9fd1e1bb199b62802c3cb98225766e0ecd1e4e08/src/Sport1.Web/Controllers/API/NetsCallbackHandler.cs


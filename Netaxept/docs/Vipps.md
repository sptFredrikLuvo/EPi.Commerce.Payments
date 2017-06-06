Support for VIPPS 
=============

Vipps is a mobile payment solution launched by DNB for online stores. With Vipps through Netaxept you can allow your customers to pay their purchases with their payment cards via Vipps app in your online store.

For more details about business features and restrictions please see netaxept_vipps_getstartedguide_en_v0.5.docx

## Known issue

During the approval process, Netaxept authorizes the payment automatically. There is a known error in the VIPPS application (iPhone related) that sometimes prevents the the callback url to be called.
If this happens the amount will be autorized but the order never created since this is part of the  PaymentCallbackController.

As a work around NETS provides a the possibility to define a custom callback url through the admin ui. See https://shop.nets.eu/web/partners/callback
When Netaxept sends a callback to the configured callback URL the transactionID is sent as a json in the request body. 


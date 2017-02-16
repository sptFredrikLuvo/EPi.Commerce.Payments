using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Geta.Commerce.Payments.PayPal.Models;
using Geta.Commerce.Payments.PayPal.Services;
using Geta.PayPal;
using Geta.PayPal.Extensions;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Customers.Profile;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Data.Provider;
using PayPal.PayPalAPIInterfaceService.Model;

namespace Geta.Commerce.Payments.PayPal
{
    public class PayPalPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        [NonSerialized]
        private readonly ILogger _logger = LogManager.GetLogger(typeof(PayPalPaymentGateway));
        private readonly Injected<IOrderRepository> _orderRepository;
        private readonly Injected<IPayPalCountryService> _countryService;
        private readonly Injected<LocalizationService> _localizationService;
        private readonly Injected<IPayPalPaymentService> _paymentService;
        private readonly Injected<IOrderGroupFactory> _orderGroupFactory;
        private readonly Injected<IOrderFormCalculator> _orderFormCalculator;
        private readonly Injected<IShippingCalculator> _shippingCalculator;
        private readonly Injected<ITaxCalculator> _taxCalculator;

        private IOrderAddress _address;
        
        private string _notifyUrl = string.Empty;

        /// <summary>
        /// Captures an authorized payment.
        /// </summary>
        /// <remarks/>
        /// <para>
        /// See API doc here https://developer.paypal.com/webapps/developer/docs/classic/api/merchant/DoCapture_API_Operation_SOAP/
        /// </para>
        /// <param name="payment">The payment to process</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        private bool ProcessPaymentCapture(IPayment payment, ref string message)
        {
            // Implement refund feature logic for current payment gateway
            var po = OrderGroup as IPurchaseOrder;

            if (po == null || payment.Amount <= 0)
            {
                message = "Nothing to capture";
                return false;
            }

            //// Update payment amount to bypass incorrect calculations in CapturePaymentActivity.
            //payment.Amount = _orderFormCalculator.Service.GetTotal(_orderForm, OrderGroup.Market, OrderGroup.Currency).Amount;

            string errorMessage;
            var response = _paymentService.Service.CapturePayment(payment, OrderGroup, out errorMessage);
           
            // Check for possible error messages
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                message = errorMessage;
                return false;
            }

            // Extract the response details.
            payment.ProviderTransactionID = response.DoCaptureResponseDetails.PaymentInfo.TransactionID;
            payment.Status = response.DoCaptureResponseDetails.PaymentInfo.PaymentStatus.ToString();

            // Add a new order note about this refund
            var orderNote = _orderGroupFactory.Service.CreateOrderNote(po);
            orderNote.Created = DateTime.Now;
            orderNote.CustomerId = po.CustomerId;
            orderNote.Detail = BuildCaptureMessage(payment,response);
            orderNote.Title = "CAPTURE";
            orderNote.Type = OrderNoteTypes.System.ToString();

            po.Notes.Add(orderNote);

            // Save
            _orderRepository.Service.Save(po);

            return true;
        }

        private string BuildCaptureMessage(IPayment payment, DoCaptureResponseType response)
        {
            var paymentInfo = response.DoCaptureResponseDetails.PaymentInfo;

            string result = string.Format("[{0}] [Capture payment-{1}] [Status: {6}] .Response: {2} at Timestamp={3}: {4}{5}",
                payment.PaymentMethodName,
                paymentInfo.TransactionID,
                response.Ack, response.Timestamp,
                paymentInfo.GrossAmount.value, paymentInfo.GrossAmount.currencyID,
                paymentInfo.PaymentStatus
            );

            return result;
        }


        /// <summary>
        /// Process payment when a refund request happens
        /// </summary>
        /// <remarks>
        /// <para>
        /// See API doc here https://www.x.com/developers/paypal/documentation-tools/api/refundtransaction-api-operation-soap
        /// </para>
        /// <para>
        /// You may offer a refund only for a limited time, usually 60 days. If you need to make a refund after that time, you will need to initiate a new PayPal payment to your buyer.
        /// If you offer the buyer a partial refund, she has 10 days to decline it if she wishes. (Full refunds are automatically processed.)
        /// </para>
        /// </remarks>
        /// <param name="payment">The payment to process</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        private bool ProcessPaymentRefund(IPayment payment, ref string message)
        {
            // Implement refund feature logic for current payment gateway
            var refundAmount = payment.Amount;
            var po = OrderGroup as PurchaseOrder;

            if (po == null || refundAmount <= 0)
            {
                message = "Nothing to refund";  // TODO: localize
                return false;
            }

            var currency = _countryService.Service.GetCurrencyCode(payment, OrderGroup.Currency);
            var invoiceNumber = po.TrackingNumber;
            var ppTransactionID = payment.ProviderTransactionID;

            #region call payment gateway API to do refund business

            // Create the Refund Request
            var refundRequest = new RefundTransactionRequestType();
            refundRequest.TransactionID = ppTransactionID;  // original transactionID (which payPal gave us when do ExpressCheckout)
            refundRequest.Memo = string.Format("[{2}-{3}] refunds {0}{1} for [PurchaseOrder-{4}]", 
                refundAmount, po.BillingCurrency,
                payment.PaymentMethodName, payment.TransactionID, 
                invoiceNumber
                );


            // NOTE: If RefundType is Full, do not set the amount.
            // refundRequest.RefundType = RefundType.Full;  //refund a full or partial amount
            refundRequest.RefundType = RefundType.PARTIAL;  //refund a partial amount
            refundRequest.Amount = refundAmount.ToPayPalAmount(currency);   // if refund with Partial, we have to set the Amount

            var caller = _paymentService.Service.GetPayPalAPICallerServices();
            var refundResponse = caller.RefundTransaction(new RefundTransactionReq { RefundTransactionRequest = refundRequest });
            string errorCheck = refundResponse.CheckErrors();

            if (string.IsNullOrEmpty(errorCheck) == false)
            {
                message = errorCheck;
                return false;
            }

            // Extract the response details.
            payment.TransactionID = refundResponse.RefundTransactionID;
            //payment.AcceptChanges();

            message =
                $"[{payment.PaymentMethodName}] [RefundTransaction-{refundResponse.RefundTransactionID}] Response: {refundResponse.Ack} at Timestamp={refundResponse.Timestamp}: {refundResponse.GrossRefundAmount.value}{refundResponse.GrossRefundAmount.currencyID}";

            // add a new order note about this refund
            var orderNote = po.OrderNotes.AddNew();
            orderNote.Created = DateTime.Now;
            orderNote.CustomerId = po.CustomerId;
            orderNote.Detail = message;
            orderNote.Title = "REFUND"; 
            orderNote.Type = OrderNoteTypes.System.ToString();
            po.AcceptChanges();
                
            return true;

            #endregion

        }

        /// <summary>
        /// Process payment when user checkout
        /// </summary>
        /// <param name="payment">The payment to process</param>
        /// <param name="message">The message.</param>
        /// <param name="cart">The current cart.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        private bool ProcessPaymentCheckout(IPayment payment, ref string message, ICart cart)
        {
            var payPalPayment = payment as PayPalPayment;
            string orderNumberID = GenerateOrderNumber(cart.OrderLink.OrderGroupId);

            var urls = UrlSettings.Create(orderNumberID, this.Settings);

            // Create CallerServices and fill its APIProfile with Commerce's PayPal security credentials
            var caller = _paymentService.Service.GetPayPalAPICallerServices();
            
            if (caller == null || string.IsNullOrEmpty(this.Settings[PayPalConstants.Configuration.ExpChkoutURLParameter]))
            {
                Utilities.UpdateCartInstanceId(cart);
                message = Translate("PayPalSettingsError");
                return false;   // raise exception
            }


            // Create the PayPal API SOAP request object.
            SetExpressCheckoutRequestType setChkOutrequest = new SetExpressCheckoutRequestType();

            // Add request-specific fields to the request.
            // Create the request details object.
            setChkOutrequest.SetExpressCheckoutRequestDetails = new SetExpressCheckoutRequestDetailsType();
            if (this.Settings[PayPalConstants.Configuration.PaymentActionParameter] == "Authorization")
            {
                setChkOutrequest.SetExpressCheckoutRequestDetails.PaymentAction = PaymentActionCodeType.AUTHORIZATION;
            }
            else if (this.Settings[PayPalConstants.Configuration.PaymentActionParameter] == "Sale")
            {
                setChkOutrequest.SetExpressCheckoutRequestDetails.PaymentAction = PaymentActionCodeType.SALE;
            }

            if (this.Settings[PayPalConstants.Configuration.AllowChangeAddressParameter] != "1")
            {
                setChkOutrequest.SetExpressCheckoutRequestDetails.AddressOverride = "1";
            }
            setChkOutrequest.SetExpressCheckoutRequestDetails.BillingAddress = GetBillingAddress(payPalPayment);
            setChkOutrequest.SetExpressCheckoutRequestDetails.BuyerEmail = _address.Email;
            setChkOutrequest.SetExpressCheckoutRequestDetails.PaymentDetails = new List<PaymentDetailsType>() { GetPaymentDetailsType(payPalPayment, orderNumberID) };
            if (this.Settings[PayPalConstants.Configuration.AllowGuestParameter] == "1")
            {
                setChkOutrequest.SetExpressCheckoutRequestDetails.SolutionType = SolutionTypeType.SOLE;
                setChkOutrequest.SetExpressCheckoutRequestDetails.LandingPage = LandingPageType.BILLING;
            }
            setChkOutrequest.SetExpressCheckoutRequestDetails.CancelURL = urls.CancelUrl;
            setChkOutrequest.SetExpressCheckoutRequestDetails.ReturnURL = urls.AcceptUrl;


            // Execute the API operation and obtain the response.

            SetExpressCheckoutResponseType setChkOutResponse = caller.SetExpressCheckout(new SetExpressCheckoutReq { SetExpressCheckoutRequest = setChkOutrequest });
            string errorCheck = setChkOutResponse.CheckErrors();
            if (string.IsNullOrEmpty(errorCheck) == false)
            {
                // TODO: Utilities.UpdateCartInstanceId(cart);
                message = errorCheck;
                _logger.Error(message);
                return false;   // raise exception
            }
            
            // validation checking with PayPal OK (Server's PayPal API, Billing Address, Shipping Address, ... do redirect to PayPal.com
            string redirectUrl = this.Settings[PayPalConstants.Configuration.ExpChkoutURLParameter];
            redirectUrl = UriSupport.AddQueryString(redirectUrl, "cmd", "_express-checkout");
            redirectUrl = UriSupport.AddQueryString(redirectUrl, "token", setChkOutResponse.Token);
            redirectUrl = UriSupport.AddQueryString(redirectUrl, "useraction", "commit");

            payPalPayment.PayPalOrderNumber = orderNumberID;
            payPalPayment.PayPalExpToken = setChkOutResponse.Token;

            Utilities.UpdateCartInstanceId(cart);
            message = $"---PayPal-SetExpressCheckout is successful. Redirect end user to {redirectUrl}";
            _logger.Information(message);
            RedirectParentPage(redirectUrl);

            return true;
        }



        /// <summary>
        /// Processes the successful transaction, call by NotifyPage (PayPal.aspx) when PayPal.com redirect back.
        /// </summary>
        /// <param name="payPalPayment">PayPal payment object, which contains Order number and Exp Token.</param>
        public ProcessTransactionResult ProcessSuccessfulTransaction(PayPalPayment payPalPayment)
        {
            #region pre-condition checking
            IPurchaseOrder po;

            if (HttpContext.Current == null)
            {
                return new ProcessTransactionResult
                {
                    Success = false,
                    Message = "No HTTP context present."
                };
            }

            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove("LastCouponCode");
            }

            var cart = OrderGroup as ICart;

            if (cart == null)
            {
                //return to the shopping cart page immediately and show error messages
                return ProcessUnsuccessfulTransaction(Translate("CommitTranErrorCartNull"));
            }

            #endregion

            if (_orderForm == null)
            {
                _orderForm = cart.Forms.FirstOrDefault(form => form.Payments.Contains(payPalPayment));
            }

            // Make sure to execute within transaction
            using (TransactionScope scope = new TransactionScope())
            {
                PaymentMethodDto payPal = _paymentService.Service.GetPayPalPaymentMethod();
                
                var caller = _paymentService.Service.GetPayPalAPICallerServices();

                #region Create the GetExpressCheckoutDetails PayPal API request object.

                GetExpressCheckoutDetailsRequestType getDetailRequest = new GetExpressCheckoutDetailsRequestType();

                // Add request-specific fields to the request.
                getDetailRequest.Token = payPalPayment.PayPalExpToken;

                // Execute the API operation and obtain the response.
                GetExpressCheckoutDetailsResponseType getDetailsResponse = caller.GetExpressCheckoutDetails(new GetExpressCheckoutDetailsReq { GetExpressCheckoutDetailsRequest = getDetailRequest });
                string errorCheck = getDetailsResponse.CheckErrors();

                if (string.IsNullOrEmpty(errorCheck) == false)
                {
                    // TODO: Utilities.UpdateCartInstanceId(cart);
                    // unsuccessful getdetail call
                    return ProcessUnsuccessfulTransaction(!string.IsNullOrEmpty(errorCheck) ? errorCheck : Translate("GetExpressCheckoutDetailsError"));
                }

                // get commerceOrderId from what we put to PayPal instead of getting from cookie
                payPalPayment.PayPalOrderNumber = getDetailsResponse.GetExpressCheckoutDetailsResponseDetails.InvoiceID;

                //process details sent from paypal, changing addresses if required
                string exChkOutErrorMessage;
                string billAddEmptyMsg = null, shipAddEmptyMsg = null;
                AddressType newShippingAddressFromPayPal = null;
                IOrderAddress modifiedBillingAddress = null, modifiedShippingAddress = null;

                if (ProcessExChkOutDetails(getDetailsResponse, cart, payPalPayment, ref billAddEmptyMsg,
                    ref shipAddEmptyMsg, ref modifiedBillingAddress, ref modifiedShippingAddress,
                    ref newShippingAddressFromPayPal, out exChkOutErrorMessage) == false)
                {
                    return ProcessUnsuccessfulTransaction(exChkOutErrorMessage);
                }

                #endregion


                #region Create the DoExpressCheckoutPayment PayPal API request object, to complete the transaction
                
                // Create the request object.
                var doCheckOutRequest = new DoExpressCheckoutPaymentRequestType();

                // Add request-specific fields to the request.
                // Create the request details object.
                doCheckOutRequest.DoExpressCheckoutPaymentRequestDetails = new DoExpressCheckoutPaymentRequestDetailsType
                {
                    Token = payPalPayment.PayPalExpToken,
                    PayerID = getDetailsResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID
                };

                if (payPal.GetParameterValueByName(PayPalConstants.Configuration.PaymentActionParameter) == "Authorization")
                {
                    doCheckOutRequest.DoExpressCheckoutPaymentRequestDetails.PaymentAction = PaymentActionCodeType.AUTHORIZATION;
                }
                else if (payPal.GetParameterValueByName(PayPalConstants.Configuration.PaymentActionParameter) == "Sale")
                {
                    doCheckOutRequest.DoExpressCheckoutPaymentRequestDetails.PaymentAction = PaymentActionCodeType.SALE;
                }

                PaymentDetailsType sentDetails = GetPaymentDetailsType(cart.GetFirstForm().Payments.FirstOrDefault(), payPalPayment.PayPalOrderNumber);
                doCheckOutRequest.DoExpressCheckoutPaymentRequestDetails.PaymentDetails = new List<PaymentDetailsType> { sentDetails };

                if (newShippingAddressFromPayPal != null)
                {
                    doCheckOutRequest.DoExpressCheckoutPaymentRequestDetails.PaymentDetails[0].ShipToAddress = newShippingAddressFromPayPal;
                }

                // Execute the API operation and obtain the response.
                DoExpressCheckoutPaymentResponseType doCheckOutResponse = caller.DoExpressCheckoutPayment(new DoExpressCheckoutPaymentReq { DoExpressCheckoutPaymentRequest = doCheckOutRequest });

                errorCheck = doCheckOutResponse.CheckErrors();

                if (string.IsNullOrEmpty(errorCheck) == false)
                {
                    // TODO: Utilities.UpdateCartInstanceId(cart);
                    // unsuccessful doCheckout response
                    return ProcessUnsuccessfulTransaction(!string.IsNullOrEmpty(errorCheck) ? errorCheck : Translate("CommitTranErrorDoExpressCheckoutPayment"));
                }

                #endregion

                // everything is fine, this is a flag to tell ProcessPayment know about this case: redirect back from PayPal with accepted payment
                payPalPayment.Status = PayPalConstants.Status.PaymentStatusCompleted;

                PaymentStatusManager.ProcessPayment(payPalPayment);
                _orderForm.UpdatePaymentTotals();

                // Saving addresses for logined users
                TrySaveAddressForLoginedUser(modifiedShippingAddress, modifiedBillingAddress, cart,payPalPayment);

                // Save changes
                //cart.OrderNumberMethod = new Cart.CreateOrderNumber((c) => payPalPayment.PayPalOrderNumber);
                
                var poReference = _orderRepository.Service.SaveAsPurchaseOrder(cart);
                po = _orderRepository.Service.Load(poReference) as IPurchaseOrder;
                po.OrderNumber = payPalPayment.PayPalOrderNumber;
     
                _orderRepository.Service.Save(po);

                //PurchaseOrder po = cart.SaveAsPurchaseOrder();  // NOTE: this might cause problem when checkout using multiple shipping address (because ECF workflow does not handle it. Modify the workflow instead of modify in this payment)
                // NOTE: po.TrackingNumber hold the orderNumber

                // read PayPal's transactionId here, in order to store it, refund ...
                var ppTransactionId = doCheckOutResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID;
                UpdateTransactionIdOfPaymentMethod(po, payPal, ppTransactionId);

                // Update last order datetime for CurrentContact
                UpdateLastOrderOfCurrentContact(PrincipalInfo.CurrentPrincipal.GetCustomerContact(), po.Created);

                AddNoteToPurchaseOrder("New order placed by {0} in {1}", po, PrincipalInfo.CurrentPrincipal.Identity.Name, "Public site");                
                
                // Remove old cart
                _orderRepository.Service.Delete(cart.OrderLink);

                // Update display name of product by current language
                po.UpdateDisplayNameWithCurrentLanguage();

                //po.AcceptChanges();

                // Commit changes
                scope.Complete();
            }   // end ECF transaction scope

            _logger.Information("PayPal transaction succeeds");

            return new ProcessTransactionResult
            {
                Success = true,
                PurchaseOrder = po
            };
        }

        /// <summary>
        /// Adds the note to purchase order.
        /// </summary>
        /// <param name="note">Name of the note.</param>
        /// <param name="purchaseOrder">The purchase order.</param>
        /// <param name="parmeters">The parmeters.</param>
        protected void AddNoteToPurchaseOrder(string note, IPurchaseOrder purchaseOrder, params string[] parmeters)
        {
            var noteDetail = string.Format(note, parmeters);
            var orderNote = _orderGroupFactory.Service.CreateOrderNote(purchaseOrder);
            orderNote.Detail = noteDetail;
            orderNote.Type = OrderNoteTypes.System.ToString();
            orderNote.CustomerId = PrincipalInfo.CurrentPrincipal.GetContactId();
            purchaseOrder.Notes.Add(orderNote);

        }

        /// <summary>
        /// loop through all payments in the PurchaseOrder, find payment of paymentMethod type, set TransactionId
        /// </summary>
        /// <param name="po">PurchaseOrder need to update</param>
        /// <param name="paymentMethod">e.g. PayPal</param>
        /// <param name="paymentGatewayTransactionID">transactionId from PaymentGateway, e.g. : from PayPal TransactionID</param>
        private void UpdateTransactionIdOfPaymentMethod(IPurchaseOrder po, PaymentMethodDto paymentMethod, string paymentGatewayTransactionID)
        {
            if (paymentMethod == null || paymentMethod.PaymentMethod.Rows.Count <= 0)
            {
                return;
            }

            // take from first row
            var guidPaymentMethodId = (Guid)paymentMethod.PaymentMethod.Rows[0][paymentMethod.PaymentMethod.PaymentMethodIdColumn];

            // loop through all payments in the PurchaseOrder, find payment with id equal guidPaymentMethodId, set TransactionId
            foreach (IOrderForm orderForm in po.Forms)
            {
                foreach (IPayment payment in orderForm.Payments)
                {
                    if (payment.PaymentMethodId.Equals(guidPaymentMethodId))
                    {
                        payment.TransactionID = paymentGatewayTransactionID;
                        payment.ProviderTransactionID = paymentGatewayTransactionID;
                    }
                }
            }

            _orderRepository.Service.Save(po);
        }


        /// <summary>
        /// If current user is not anonymous, try to save the ShippingAddress and BillingAddress to its Contact
        /// </summary>
        /// <param name="modifiedShippingAddress">shipping address to save</param>
        /// <param name="modifiedBillingAddress">billing address to save</param>
        /// <param name="cart">if modifiedShippingAddress and/or modifiedBillingAddress is null, try to take addresses from Cart.OrderForm</param>
        /// <param name="payment"></param>
        private void TrySaveAddressForLoginedUser(IOrderAddress modifiedShippingAddress, IOrderAddress modifiedBillingAddress, ICart cart,IPayment payment)
        {
            if (HttpContext.Current == null)
            {
                return;
            }
            var httpProfile = HttpContext.Current.Profile;
            var profile = httpProfile == null ? null : new CustomerProfileWrapper(httpProfile);
            if (profile != null && !profile.IsAnonymous)
            {
                //the new shipping address from paypal, save it as a new customer address
                IOrderAddress shipAdd = modifiedShippingAddress ?? cart.GetFirstShipment().ShippingAddress;
                SaveCustomerAddress(shipAdd, CustomerAddressTypeEnum.Shipping);

                //the new billing address from PayPal, as it as new customer address
                IOrderAddress billAdd = modifiedBillingAddress ?? payment.BillingAddress;
                SaveCustomerAddress(billAdd, CustomerAddressTypeEnum.Billing);
            }
        }

        /// <summary>
        /// Update last order timestamp which current user completed
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="datetime"></param>
        private void UpdateLastOrderOfCurrentContact(CustomerContact contact, DateTime datetime)
        {
            if (contact != null)
            {
                contact.LastOrder = datetime;
                contact.SaveChanges();
            }
        }


        /// <summary>
        /// Processes the unsuccessful transaction, delete PayPal-related information in Cookie.
        /// </summary>
        /// <param name="errorMessage"></param>
        public ProcessTransactionResult ProcessUnsuccessfulTransaction(string errorMessage)
        {
            if (HttpContext.Current == null)
            {
                return new ProcessTransactionResult
                {
                    Success = false,
                    Message = "No HTTP context present."
                };
            }

            string payPalErrorMessage = $"PayPal transaction failed [{errorMessage}]";

            _logger.Error(payPalErrorMessage);

            return new ProcessTransactionResult
            {
                Success = false,
                Message = payPalErrorMessage
            };
        }

        /// <summary>
        /// Saves the new order address to a (new) customer contact if not already existed.
        /// </summary>
        /// <param name="address">The address to save</param>
        /// <param name="type">Customer address type</param>
        private void SaveCustomerAddress(IOrderAddress address, CustomerAddressTypeEnum type)
        {
            // Add to contact address
            var customerContact = PrincipalInfo.CurrentPrincipal.GetCustomerContact();
            if (customerContact != null)
            {
                CustomerAddress customerAddress = StoreHelper.ConvertToCustomerAddress((OrderAddress)address);
                if (customerContact.ContactAddresses == null || !StoreHelper.IsAddressInCollection(customerContact.ContactAddresses, customerAddress))
                {
                    customerAddress.AddressType = type;
                    customerAddress.Name = customerAddress.Name.StripPreviewText(46);
                    customerAddress.FirstName = customerAddress.FirstName.StripPreviewText(60);
                    customerAddress.LastName = customerAddress.LastName.StripPreviewText(60);
                    customerAddress.Line1 = customerAddress.Line1.StripPreviewText(76);
                    customerAddress.Line2 = customerAddress.Line2.StripPreviewText(76);

                    customerContact.AddContactAddress(customerAddress);
                    customerContact.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Processes the checkout details from PayPal, update changed addresses appropriately
        /// </summary>
        /// <param name="chkOutDetailsResponse">The CHK out details response.</param>
        /// <param name="cart">The cart.</param>
        /// <param name="payment"></param>
        /// <param name="billAddEmptyMsg">The bill add empty MSG.</param>
        /// <param name="shipAddEmptyMsg">The ship add empty MSG.</param>
        /// <param name="modifiedBillingAddress"></param>
        /// <param name="modifiedShippingAddress"></param>
        /// <param name="newShippingAddressFromPayPal"></param>
        /// <param name="errorMessage"></param>
        private bool ProcessExChkOutDetails(GetExpressCheckoutDetailsResponseType chkOutDetailsResponse, ICart cart, IPayment payment,
            ref string billAddEmptyMsg, ref string shipAddEmptyMsg, ref IOrderAddress modifiedBillingAddress,
            ref IOrderAddress modifiedShippingAddress, ref AddressType newShippingAddressFromPayPal, out string errorMessage)
        {
            GetExpressCheckoutDetailsResponseDetailsType details = chkOutDetailsResponse.GetExpressCheckoutDetailsResponseDetails;
            AddressType payPalBillAdd = details.BillingAddress;

            //update billing address
            if (payPalBillAdd != null)
            {
                IOrderAddress billAdd = payment.BillingAddress;//new CartHelper(cart).FindAddressByName(cart.OrderForms[0].BillingAddressId);

                if (billAdd == null)
                {
                    // TODO: Utilities.UpdateCartInstanceId(cart);
                    errorMessage = LocalizationService.GetString("CommitTranErrorCartReset");
                    return false;
                }

                if (string.IsNullOrEmpty(payPalBillAdd.Phone))
                {
                    if (details.PayerInfo != null && !string.IsNullOrEmpty(details.PayerInfo.ContactPhone))
                    {
                        payPalBillAdd.Phone = details.PayerInfo.ContactPhone;
                    }
                }

                if (IsAddressChanged(billAdd, payPalBillAdd))
                {
                    billAdd.City = payPalBillAdd.CityName;
                    billAdd.CountryCode = _countryService.Service.GetAlpha3CountryCode(payPalBillAdd.Country.ToString().ToUpperInvariant());
                    billAdd.DaytimePhoneNumber = payPalBillAdd.Phone;
                    billAdd.EveningPhoneNumber = payPalBillAdd.Phone;
                    billAdd.Line1 = payPalBillAdd.Street1;
                    billAdd.Line2 = payPalBillAdd.Street2;
                    billAdd.PostalCode = payPalBillAdd.PostalCode;
                    billAdd.RegionName = _countryService.Service.GetStateName(payPalBillAdd.StateOrProvince);
                    billAdd.Email = details.PayerInfo.Payer;
                    string name = payPalBillAdd.Name.Trim();
                    billAdd.Id = name.StripPreviewText(46);
                    int index = name.IndexOf(' ');
                    billAdd.FirstName = index >= 0 ? name.Substring(0, index) : name;
                    billAdd.LastName = index >= 0 ? name.Substring(index + 1) : string.Empty;
                    modifiedBillingAddress = billAdd;

                    // Update billingAddressId when billing address was changed
                    //cart.OrderForms[0].BillingAddressId = billAdd.Name;
                    payment.BillingAddress = billAdd;

                }
            }
            else
            {
                billAddEmptyMsg = Translate("CommitTranErrorPayPalBillingAddressEmpty");
            }

            AddressType payPalShipAdd = details.PaymentDetails[0].ShipToAddress;

            // update shipping address
            if (payPalShipAdd != null)
            {
                var shipAdd = cart.GetFirstShipment().ShippingAddress;//cartHelper.FindAddressByName(cart.OrderForms[0].Shipments[0].ShippingAddressId);

                if (shipAdd == null)
                {
                    // TODO: Utilities.UpdateCartInstanceId(cart);
                    errorMessage = Translate("CommitTranErrorCartReset");
                    return false;
                }

                if (string.IsNullOrEmpty(payPalShipAdd.Phone))
                {
                    if (details.PayerInfo != null && !string.IsNullOrEmpty(details.PayerInfo.ContactPhone))
                    {
                        payPalShipAdd.Phone = details.PayerInfo.ContactPhone;
                    }
                }

                if (IsAddressChanged(shipAdd, payPalShipAdd))
                {
                    string name = payPalShipAdd.Name.Trim().StripPreviewText(46);
                    // add new shipping address to OrderAddresses, so it can be found later, when getting shipping address.
                    // or update the existing one, in case the address was updated in PayPal
                    var existingShippingAddress = OrderGroup.GetFirstShipment().ShippingAddress;//cartHelper.FindAddressByName(name);
                    if (existingShippingAddress == null)
                    {
                        shipAdd = new OrderAddress();
                    }
                    else
                    {
                        // keep the original order address untouch, otherwise it might update/remove the billing address
                        var addressEntity = new AddressEntity();
                        OrderAddress.CopyOrderAddressToCustomerAddress((OrderAddress)existingShippingAddress, addressEntity);
                        shipAdd = new OrderAddress(addressEntity);
                    }

                    shipAdd.City = payPalShipAdd.CityName;
                    shipAdd.CountryCode = _countryService.Service.GetAlpha3CountryCode(payPalShipAdd.Country.ToString().ToUpperInvariant());
                    shipAdd.DaytimePhoneNumber = payPalShipAdd.Phone;
                    shipAdd.EveningPhoneNumber = payPalShipAdd.Phone;
                    shipAdd.Line1 = payPalShipAdd.Street1;
                    shipAdd.Line2 = payPalShipAdd.Street2;
                    shipAdd.PostalCode = payPalShipAdd.PostalCode;
                    shipAdd.RegionName = _countryService.Service.GetStateName(payPalShipAdd.StateOrProvince);
                    shipAdd.Email = details.PayerInfo.Payer;
                    shipAdd.Id = name;
                    int index = name.IndexOf(' ');
                    shipAdd.FirstName = index >= 0 ? name.Substring(0, index) : name;
                    shipAdd.LastName = index >= 0 ? name.Substring(index + 1) : string.Empty;

                    // Update shippingAddressId when shipping address was changed
                    OrderGroup.GetFirstShipment().ShippingAddress.Id = shipAdd.Id;

                    if (existingShippingAddress == null)
                    {
                        modifiedShippingAddress = shipAdd;
                        // cart.OrderAddresses.Add(shipAdd);
                        cart.GetFirstShipment().ShippingAddress = shipAdd;
                    }
                }

                newShippingAddressFromPayPal = payPalShipAdd;
            }
            else
            {
                shipAddEmptyMsg = LocalizationService.GetString("CommitTranErrorPayPalShippingAddressEmpty");
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Determines whether the address is changed.
        /// </summary>
        /// <param name="orderAddress">The order address.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <returns>
        /// 	<c>true</c> if [is address changed] [the specified order address]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAddressChanged(IOrderAddress orderAddress, AddressType addressType)
        {
            if (!string.Equals(orderAddress.City, addressType.CityName, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(orderAddress.CountryCode, _countryService.Service.GetAlpha3CountryCode(addressType.Country.ToString().ToUpperInvariant()), StringComparison.OrdinalIgnoreCase)
                || (!string.Equals(orderAddress.DaytimePhoneNumber ?? string.Empty, addressType.Phone ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                || !string.Equals(orderAddress.Line1, addressType.Street1, StringComparison.OrdinalIgnoreCase)
                || (!string.Equals(orderAddress.Line2 ?? string.Empty, addressType.Street2 ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                || !string.Equals(orderAddress.PostalCode, addressType.PostalCode, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(orderAddress.RegionName, _countryService.Service.GetStateName(addressType.StateOrProvince), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

       


        /// <summary>
        /// Construct the Gets the PayPal payment details from our payment and Cart to pass onto PayPal.
        /// </summary>
        /// <remarks>
        /// <p>the PayPal payment detail can be a bit different from OrderForm because 
        /// sometimes cart total calculated by Commerce is different with cart total calculated by PalPal, 
        /// though this difference is very small (~0.01 currency).
        /// We adjust this into an addtional item to ensure PayPal shows the same total number with Commerce.
        /// </p>
        /// <p>
        /// We also add the Order discount (if any) as and special item with negative price to PayPal payment detail
        /// </p>
        /// <p>
        /// See detail about PayPal struct type in this link <seealso cref="https://cms.paypal.com/mx/cgi-bin/?cmd=_render-content&content_ID=developer/e_howto_api_soap_r_GetExpressCheckoutDetails#id0848IH0064Y__id099MB0BB0UX"/>
        /// </p>
        /// </remarks>
        /// <param name="payment">the payment to take info (Total, LineItem, ...) from</param>
        /// <param name="orderNumber">the orderNumber (to be InvoiceID to pass to PayPal)</param>
        /// <returns>the PayPal payment detail to pass to API request</returns>
        private PaymentDetailsType GetPaymentDetailsType(IPayment payment, string orderNumber)
        {
            var ppPayment = new PaymentDetailsType();

            ppPayment.ButtonSource = "Episerver_Cart_EC"; // (Optional) An identification code for use by third-party applications to identify transactions. Character length and limitations: 32 single-byte alphanumeric characters
            ppPayment.InvoiceID = orderNumber;  // Character length and limitations: 127 single-byte alphanumeric characters
            ppPayment.Custom = OrderGroup.CustomerId + "|" + ppPayment.InvoiceID; // A free-form field for your own use. Character length and limitations: 256 single-byte alphanumeric characters
            // NOTE: ppPayment.OrderDescription = 127 single-byte alphanumeric characters string
            // NOTE: ppPayment.TransactionId = string, provided if you have transactionId in your Commerce system // (Optional) Transaction identification number of the transaction that was created.;

            // (Optional) Your URL for receiving Instant Payment Notification (IPN) about this transaction. If you do not specify this value in the request, the notification URL from your Merchant Profile is used, if one exists. 
            // IMPORTANT:The notify URL only applies to DoExpressCheckoutPayment. This value is ignored when set in SetExpressCheckout or GetExpressCheckoutDetails.
            // Character length and limitations: 2,048 single-byte alphanumeric characters
            ppPayment.NotifyURL = _notifyUrl;

            var currencyCode = _countryService.Service.GetCurrencyCode(payment,OrderGroup.Currency);
            var currency = new Currency(currencyCode.ToString());

            decimal totalOrder = currency.Round(payment.Amount);
            decimal totalShipping = currency.Round(_shippingCalculator.Service.GetShippingCost(_orderForm, OrderGroup.Market, currency).Amount);
            decimal totalHandling = currency.Round(_orderFormCalculator.Service.GetHandlingTotal(_orderForm, currency).Amount);
            decimal totalTax = currency.Round(_taxCalculator.Service.GetTaxTotal(_orderForm, OrderGroup.Market, currency).Amount);
            
            decimal lineitemTotal = 0;

            var ppItemDetails = new List<PaymentDetailsItemType>();
            var lineItems = _orderForm.GetAllLineItems();

            for (int i = 0; i < lineItems.Count(); i++)
            {
                var lineitem = lineItems.ElementAt(i);
                
                var ppItemDetail = new PaymentDetailsItemType();
                ppItemDetail.Name = lineitem.DisplayName;
                ppItemDetail.Number = lineitem.Code;
                //ppItemDetail.Description = lineitem.Description.StripPreviewText(100);  // Character length and limitations: 127 single-byte characters
                ppItemDetail.Quantity = Convert.ToInt32(lineitem.Quantity.ToString("0"));
                // recalculate final unit price after all kind of discounts are subtracted from item.ListPrice
                var extendedPrice = lineitem.GetExtendedPrice(currency);
                decimal finalUnitPrice = currency.Round(extendedPrice.Amount / lineitem.Quantity); 
                ppItemDetail.Amount = finalUnitPrice.ToPayPalAmount(currencyCode);
                
                lineitemTotal += finalUnitPrice * lineitem.Quantity;
                ppItemDetails.Add(ppItemDetail);
            }

            #region Calculate the adjustment, if needed
            
            // this adjustment also include the giftcard (in sample)
            decimal orderAdjustment = totalOrder - totalShipping - totalHandling - totalTax - lineitemTotal;
            decimal adjustmentForShipping = 0;
            if (orderAdjustment != 0 || // adjustment for gift card/(order level) promotion case
                
                lineitemTotal == 0  // in this case, the promotion (or discount) make all lineItemTotal zero, but buyer still have to pay shipping (and/or handling, tax). 
                // We still need to adjust lineItemTotal for Paypal accepting (need to be greater than zero)
                )
            {
                var ppItemDetail = new PaymentDetailsItemType();
                ppItemDetail.Name = "Order adjustment";
                ppItemDetail.Number = "ORDERADJUSTMENT";
                ppItemDetail.Description = "GiftCard, Discount at OrderLevel and/or PayPal-Commerce-calculating difference in cart total";  // Character length and limitations: 127 single-byte characters
                ppItemDetail.Quantity = 1;

                var predictLineitemTotal = lineitemTotal + orderAdjustment;

                if (predictLineitemTotal <= 0)
                {
                    // can't overpaid for item. E.g.: total item amount is 68, orderAdjustment is -70, PayPal will refuse ItemTotal = -2
                    // we need to push -2 to shippingTotal or shippingDiscount

                    // HACK: Paypal will not accept an item total of $0, even if there is a shipping fee. The Item total must be at least 1 cent/penny. 
                    // We need to take 1 cent/penny from adjustmentForLineItemTotal and push to adjustmentForShipping
                    orderAdjustment = (-lineitemTotal + 0.01m); // -68 + 0.01 = -67.99
                    adjustmentForShipping = predictLineitemTotal - 0.01m;   // -2 - 0.01 = -2.01
                }
                else
                {
                    // this case means: due to PayPal calculation, buyer need to pay more that what Commerce calculate. Because:
                    // sometimes cart total calculated by Commerce is different with
                    // cart total calculated by PalPal, though this difference is very small (~0.01 currency)
                    // We adjust the items total to make up for that, to ensure PayPal shows the same total number with Commerce
                }
                ppItemDetail.Amount = orderAdjustment.ToPayPalAmount(currencyCode);
                lineitemTotal += orderAdjustment;   // re-adjust the lineItemTotal
                ppItemDetails.Add(ppItemDetail);
            }

            if (adjustmentForShipping > 0)
            {
                totalShipping += adjustmentForShipping;
            }
            else
            {
                // Shipping discount for this order. You specify this value as a negative number.
                // NOTE:Character length and limitations: Must not exceed $10,000 USD in any currency. 
                // No currency symbol. Regardless of currency, decimal separator must be a period (.), and the optional thousands separator must be a comma (,). 
                // Equivalent to nine characters maximum for USD. 
                // NOTE:You must set the currencyID attribute to one of the three-character currency codes for any of the supported PayPal currencies.
                ppPayment.ShippingDiscount = adjustmentForShipping.ToPayPalAmount(currencyCode); 
            }

            #endregion
            
            ppPayment.OrderTotal = totalOrder.ToPayPalAmount(currencyCode);
            ppPayment.ShippingTotal = totalShipping.ToPayPalAmount(currencyCode);
            ppPayment.HandlingTotal = totalHandling.ToPayPalAmount(currencyCode);
            ppPayment.TaxTotal = totalTax.ToPayPalAmount(currencyCode);
            ppPayment.ItemTotal = lineitemTotal.ToPayPalAmount(currencyCode);
            
            ppPayment.PaymentDetailsItem = ppItemDetails; 
            ppPayment.ShipToAddress = GetShippingAddress(payment);

            if (_orderForm.Shipments != null && _orderForm.Shipments.Count > 1) 
            {
                // (Optional) The value 1 indicates that this payment is associated with multiple shipping addresses. Character length and limitations: Four single-byte numeric characters.
                ppPayment.MultiShipping = "1";
            }
            
            return ppPayment;
        }

        /// <summary>
        /// Generates the order number by "PO" + cart.OrderGroupId + Random(100-999)
        /// </summary>
        /// <param name="OrderGroupId">The OrderGroupId, used to take from from Cart.OrderGroupId</param>
        /// <returns>Random string to be OrderNumber</returns>
        private string GenerateOrderNumber(int OrderGroupId)
        {
            string str = new Random().Next(100, 999).ToString();
            return $"PO{OrderGroupId}{str}";
        }

        /// <summary>
        /// Gets the billing address.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        /// <value>The billing address.</value>
        public AddressType GetBillingAddress(IPayment payment)
        {
            AddressType billAdd = new AddressType();

            _address = payment.BillingAddress;
            billAdd.CityName = _address.City;
            billAdd.Country = _countryService.Service.GetAlpha2CountryCode(_address);
            billAdd.Street1 = _address.Line1;
            billAdd.Street2 = _address.Line2;
            billAdd.PostalCode = _address.PostalCode;
            billAdd.StateOrProvince = _countryService.Service.GetStateCode(_address.RegionName);
            billAdd.Phone = (string.IsNullOrEmpty(_address.DaytimePhoneNumber) ? _address.EveningPhoneNumber : _address.DaytimePhoneNumber);
            billAdd.Name = _address.FirstName + " " + _address.LastName;
            return billAdd;
        }

        /// <summary>
        /// Gets the shipping address.
        /// </summary>
        /// <value>The shipping address.</value>
        public AddressType GetShippingAddress(IPayment payment)
        {
            AddressType shipingAdd = new AddressType();
            var shippingAddress = _orderForm.Shipments.First().ShippingAddress;
            shipingAdd.CityName = shippingAddress.City;
            shipingAdd.Country = _countryService.Service.GetAlpha2CountryCode(shippingAddress);
            shipingAdd.Street1 = shippingAddress.Line1;
            shipingAdd.Street2 = shippingAddress.Line2;
            shipingAdd.PostalCode = shippingAddress.PostalCode;
            // TODO: check working
            shipingAdd.StateOrProvince = _countryService.Service.GetStateCode(shippingAddress.RegionName);
            shipingAdd.Phone = (string.IsNullOrEmpty(shippingAddress.DaytimePhoneNumber) ? shippingAddress.EveningPhoneNumber : shippingAddress.DaytimePhoneNumber);
            shipingAdd.Name = shippingAddress.FirstName + " " + shippingAddress.LastName;
            return shipingAdd;
        }


        #region For redirection inside iframe

        /// <summary>
        /// Redirects to PayPal.com to process payment.
        /// </summary>
        /// <param name="urlNeedToRedirectByParent"></param>
        private void RedirectParentPage(string urlNeedToRedirectByParent)
        {
            HttpContext.Current.Response.Redirect(urlNeedToRedirectByParent, true);
        }

        #endregion

        private static LocalizationService LocalizationService
        {
            get { return ServiceLocator.Current.GetInstance<LocalizationService>(); }
        }

        /// <summary>
        /// Main entry point of ECF Payment Gateway.
        /// </summary>
        /// <param name="payment">The payment to process</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            OrderGroup = payment.Parent.Parent;
            _orderForm = payment.Parent;
            return ProcessPayment(payment as IPayment, ref message);
        }

        private IOrderForm _orderForm;

        public IOrderGroup OrderGroup { get; set; }

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            #region pre-condition checking

            if (HttpContext.Current == null)
            {
                message = Translate("ProcessPaymentNullHttpContext");
                return true;
            }

            #endregion

            if (_orderForm == null)
            {
                _orderForm = OrderGroup.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
            }

            //throw (new PaymentException("error","code133","error occured in paypal!"));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var cart = OrderGroup as ICart;
            // the order which is created by Commerce Manager
            if (cart == null && OrderGroup is IPurchaseOrder)
            {
                var taxTotal = _taxCalculator.Service.GetTaxTotal(_orderForm, OrderGroup.Market, OrderGroup.Currency).Amount;

                if (taxTotal > decimal.Zero && payment.Amount == _orderForm.GetTotal(OrderGroup.Market, OrderGroup.Currency).Amount - taxTotal)
                {
                    payment.Amount += taxTotal;
                }

                #region CAPTURE

                if (payment.TransactionType == TransactionType.Capture.ToString())
                {
                    return ProcessPaymentCapture(payment, ref message);
                }

                #endregion

                #region REFUND

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (payment.TransactionType == TransactionType.Credit.ToString())
                {
                    return ProcessPaymentRefund(payment, ref message);
                }

                #endregion

                #region CREATE ORDER IN COMMERCE MANAGER

                // right now we do not support processing the order which is created by Commerce Manager
                message = "The current payment method does not support this order type.";
                return false;   // raise exception

                #endregion
            }

            #region PAID ON PAYPAL

            if (cart != null && payment.Status == PayPalConstants.Status.PaymentStatusCompleted)
            {
                // return true because this shopping cart has been paid already on PayPal
                // when program flow redirects back from PayPal to PayPal.aspx file, call ProcessSuccessfulTransaction, run WorkFlow
                message = Translate("ProcessPaymentStatusCompleted");
                return true;
            }

            #endregion

            // CHECKOUT
            return ProcessPaymentCheckout(payment, ref message, cart);
        }

       


        /// <summary>
        /// translate with languageKey under /Commerce/PayPalPayment/ in lang.xml
        /// </summary>
        /// <param name="languageKey"></param>
        /// <returns></returns>
        public string Translate(string languageKey)
        {
            return _localizationService.Service.GetString("/Commerce/PayPalPayment/" + languageKey);
        }

        
    }
}

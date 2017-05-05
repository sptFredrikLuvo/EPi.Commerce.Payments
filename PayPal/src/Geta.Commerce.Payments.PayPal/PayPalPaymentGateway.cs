using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Geta.Commerce.Payments.PayPal.Helpers;
using Geta.PayPal;
using Mediachase.Commerce.Core.Features;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Extensions;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Plugins.Payment;
using Mediachase.Commerce.Security;
using Mediachase.Data.Provider;
using PayPal.PayPalAPIInterfaceService.Model;

namespace Geta.Commerce.Payments.PayPal
{
    public class PayPalPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(PayPalPaymentGateway));
        private const string PaymentStatusCompleted = "PayPal payment completed";

        public const string PayPalOrderNumberPropertyName = "PayPalOrderNumber";
        public const string PayPalExpTokenPropertyName = "PayPalExpToken";

        private readonly IOrderRepository _orderRepository;
        private readonly IFeatureSwitch _featureSwitch;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly IOrderNumberGenerator _orderNumberGenerator;
        private readonly ITaxCalculator _taxCalculator;
        private readonly PayPalAPIHelper _payPalAPIHelper;

        private string _notifyUrl = string.Empty;
        private IOrderForm _orderForm;
        private PayPalConfiguration _paymentMethodConfiguration;

        /// <summary>
        /// Gets or sets the order group containing processing payment.
        /// </summary>
        public IOrderGroup OrderGroup { get; set; }

        public PayPalPaymentGateway()
            : this(ServiceLocator.Current.GetInstance<IFeatureSwitch>(),
                  ServiceLocator.Current.GetInstance<IInventoryProcessor>(),
                  ServiceLocator.Current.GetInstance<IOrderNumberGenerator>(),
                  ServiceLocator.Current.GetInstance<IOrderRepository>(),
                  ServiceLocator.Current.GetInstance<ITaxCalculator>(),
                  new PayPalAPIHelper())
        {
        }

        public PayPalPaymentGateway(
            IFeatureSwitch featureSwitch,
            IInventoryProcessor inventoryProcessor,
            IOrderNumberGenerator orderNumberGenerator,
            IOrderRepository orderRepository,
            ITaxCalculator taxCalculator,
            PayPalAPIHelper paypalAPIHelper)
        {
            _featureSwitch = featureSwitch;
            _inventoryProcessor = inventoryProcessor;
            _orderNumberGenerator = orderNumberGenerator;
            _orderRepository = orderRepository;
            _taxCalculator = taxCalculator;
            _payPalAPIHelper = paypalAPIHelper;

            _paymentMethodConfiguration = new PayPalConfiguration(Settings);
        }

        /// <summary>
        /// Main entry point of ECF Payment Gateway.
        /// </summary>
        /// <param name="payment">The payment to process</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            OrderGroup = payment.Parent.Parent;
            _orderForm = payment.Parent;
            return ProcessPayment(payment as IPayment, ref message);
        }

        /// <summary>
        /// Main entry point of ECF Payment Gateway.
        /// </summary>
        /// <param name="payment">The payment to process</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        public bool ProcessPayment(IPayment payment, ref string message)
        {
            if (HttpContext.Current == null)
            {
                message = Utilities.Translate("ProcessPaymentNullHttpContext");
                return true;
            }

            if (payment == null)
            {
                message = Utilities.Translate("PaymentNotSpecified");
                return false;
            }

            if (OrderGroup == null)
            {
                message = Utilities.Translate("PaymentNotAssociatedOrderGroup");
                return false;
            }

            _orderForm = _orderForm ?? OrderGroup.Forms.FirstOrDefault(f => f.Payments.Contains(payment));
            if (_orderForm == null)
            {
                message = Utilities.Translate("PaymentNotAssociatedOrderForm");
                return false;
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (string.IsNullOrEmpty(payment.TransactionType) && !string.IsNullOrEmpty(_paymentMethodConfiguration.PaymentAction))
            {
                payment.TransactionType = _paymentMethodConfiguration.PaymentAction;
                _orderRepository.Save(OrderGroup);
            }

            var cart = OrderGroup as ICart;
            // the order which is created by Commerce Manager
            if (cart == null && OrderGroup is IPurchaseOrder)
            {
                if (payment.TransactionType == TransactionType.Capture.ToString())
                {
                    return ProcessPaymentCapture(payment, OrderGroup, ref message);
                }

                // When "Refund" shipment in Commerce Manager, this method will be invoked with the TransactionType is Credit
                if (payment.TransactionType == TransactionType.Credit.ToString())
                {
                    return ProcessPaymentRefund(payment, OrderGroup, ref message);
                }

                // right now we do not support processing the order which is created by Commerce Manager
                message = "The current payment method does not support order type.";
                return false; // raise exception
            }

            if (cart != null && cart.OrderStatus == OrderStatus.Completed)
            {
                // return true because this shopping cart has been paid already on PayPal
                // when program flow redirects back from PayPal to PayPal.aspx file, call ProcessSuccessfulTransaction, run WorkFlow
                message = Utilities.Translate("ProcessPaymentStatusCompleted");
                return true;
            }

            // CHECKOUT
            return ProcessPaymentCheckout(payment, cart, ref message);
        }

        /// <summary>
        /// Processes the successful transaction, was called when PayPal.com redirect back.
        /// </summary>
        /// <param name="orderGroup">The order group that was processed.</param>
        /// <param name="payment">The order payment.</param>
        /// <param name="acceptUrl">The redirect url when finished.</param>
        /// <param name="cancelUrl">The redirect url when error happens.</param>
        /// <returns>The url redirection after process.</returns>
        public string ProcessSuccessfulTransaction(IOrderGroup orderGroup, IPayment payment, string acceptUrl, string cancelUrl)
        {
            if (HttpContext.Current == null)
            {
                return cancelUrl;
            }

            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove("LastCouponCode");
            }

            var cart = orderGroup as ICart;
            if (cart == null)
            {
                // return to the shopping cart page immediately and show error messages
                return ProcessUnsuccessfulTransaction(cancelUrl, Utilities.Translate("CommitTranErrorCartNull"));
            }

            var redirectionUrl = acceptUrl;
            using (TransactionScope scope = new TransactionScope())
            {
                var getDetailRequest = new GetExpressCheckoutDetailsRequestType
                {
                    Token = payment.Properties[PayPalExpTokenPropertyName] as string // Add request-specific fields to the request.
                };

                // Execute the API operation and obtain the response.
                var caller = PayPalAPIHelper.GetPayPalAPICallerServices(_paymentMethodConfiguration);
                var getDetailsResponse = caller.GetExpressCheckoutDetails(new GetExpressCheckoutDetailsReq
                {
                    GetExpressCheckoutDetailsRequest = getDetailRequest
                });

                var errorCheck = _payPalAPIHelper.CheckErrors(getDetailsResponse);
                if (!string.IsNullOrEmpty(errorCheck))
                {
                    // unsuccessful get detail call
                    return ProcessUnsuccessfulTransaction(cancelUrl, errorCheck);
                }

                var expressCheckoutDetailsResponse = getDetailsResponse.GetExpressCheckoutDetailsResponseDetails;
                // get commerceOrderId from what we put to PayPal instead of getting from cookie
                payment.Properties[PayPalOrderNumberPropertyName] = expressCheckoutDetailsResponse.InvoiceID;

                //process details sent from paypal, changing addresses if required
                var emptyAddressMsg = string.Empty;

                //process billing address
                var payPalBillingAddress = expressCheckoutDetailsResponse.BillingAddress;
                if (payPalBillingAddress != null && AddressHandling.IsAddressChanged(payment.BillingAddress, payPalBillingAddress))
                {
                    emptyAddressMsg = _payPalAPIHelper.ProcessOrderAddress(expressCheckoutDetailsResponse.PayerInfo, payPalBillingAddress, payment.BillingAddress, CustomerAddressTypeEnum.Billing, "CommitTranErrorPayPalBillingAddressEmpty");
                    if (!string.IsNullOrEmpty(emptyAddressMsg))
                    {
                        return ProcessUnsuccessfulTransaction(cancelUrl, emptyAddressMsg);
                    }
                }

                //process shipping address
                var payPalShippingAddress = expressCheckoutDetailsResponse.PaymentDetails[0].ShipToAddress;
                if (payPalShippingAddress != null && AddressHandling.IsAddressChanged(cart.GetFirstShipment().ShippingAddress, payPalShippingAddress))
                {
                    //when address was changed on PayPal site, it might cause changing tax value changed and changing order value also.
                    var taxValueBefore = _taxCalculator.GetTaxTotal(cart, cart.Market, cart.Currency);

                    var shippingAddress = orderGroup.CreateOrderAddress();

                    emptyAddressMsg = _payPalAPIHelper.ProcessOrderAddress(expressCheckoutDetailsResponse.PayerInfo, payPalShippingAddress, shippingAddress, CustomerAddressTypeEnum.Shipping, "CommitTranErrorPayPalShippingAddressEmpty");
                    if (!string.IsNullOrEmpty(emptyAddressMsg))
                    {
                        return ProcessUnsuccessfulTransaction(cancelUrl, emptyAddressMsg);
                    }

                    cart.GetFirstShipment().ShippingAddress = shippingAddress;

                    var taxValueAfter = _taxCalculator.GetTaxTotal(cart, cart.Market, cart.Currency);
                    if (taxValueBefore != taxValueAfter)
                    {
                        _orderRepository.Save(cart); // Saving cart to submit order address changed.
                        scope.Complete();
                        return ProcessUnsuccessfulTransaction(cancelUrl, Utilities.Translate("ProcessPaymentTaxValueChangedWarning"));
                    }
                }

                // Add request-specific fields to the request.
                // Create the request details object.
                var doExpressChkOutPaymentReqDetails = CreateExpressCheckoutPaymentRequest(getDetailsResponse, orderGroup, payment);

                // Execute the API operation and obtain the response.
                var doCheckOutResponse = caller.DoExpressCheckoutPayment(new DoExpressCheckoutPaymentReq
                {
                    DoExpressCheckoutPaymentRequest = new DoExpressCheckoutPaymentRequestType(doExpressChkOutPaymentReqDetails)
                });

                errorCheck = _payPalAPIHelper.CheckErrors(doCheckOutResponse);
                if (!string.IsNullOrEmpty(errorCheck))
                {
                    // unsuccessful doCheckout response
                    return ProcessUnsuccessfulTransaction(cancelUrl, errorCheck);
                }

                // everything is fine, this is a flag to tell ProcessPayment know about this case: redirect back from PayPal with accepted payment
                DoCompletingCart(cart);

                // Place order
                var purchaseOrder = MakePurchaseOrder(doCheckOutResponse, cart, payment);

                // Commit changes
                scope.Complete();

                redirectionUrl = CreateRedirectionUrl(purchaseOrder, acceptUrl, payment.BillingAddress.Email);
            }

            _logger.Information($"PayPal transaction succeeds, redirect end user to {redirectionUrl}");
            return redirectionUrl;
        }

        private DoExpressCheckoutPaymentRequestDetailsType CreateExpressCheckoutPaymentRequest(GetExpressCheckoutDetailsResponseType getDetailsResponse, IOrderGroup orderGroup, IPayment payment)
        {
            var checkoutDetailsResponse = getDetailsResponse.GetExpressCheckoutDetailsResponseDetails;

            var doExpressChkOutPaymentReqDetails = new DoExpressCheckoutPaymentRequestDetailsType
            {
                Token = payment.Properties[PayPalExpTokenPropertyName] as string,
                PayerID = checkoutDetailsResponse.PayerInfo.PayerID
            };

            TransactionType transactionType;
            if (Enum.TryParse(_paymentMethodConfiguration.PaymentAction, out transactionType))
            {
                if (transactionType == TransactionType.Authorization)
                {
                    doExpressChkOutPaymentReqDetails.PaymentAction = PaymentActionCodeType.AUTHORIZATION;
                }
                else if (transactionType == TransactionType.Sale)
                {
                    doExpressChkOutPaymentReqDetails.PaymentAction = PaymentActionCodeType.SALE;
                }
            }

            var sentDetails = _payPalAPIHelper.GetPaymentDetailsType(payment, orderGroup, payment.Properties[PayPalOrderNumberPropertyName] as string, _notifyUrl);
            doExpressChkOutPaymentReqDetails.PaymentDetails = new List<PaymentDetailsType>() { sentDetails };

            var newShippingAddressFromPayPal = checkoutDetailsResponse.PaymentDetails[0].ShipToAddress;
            if (newShippingAddressFromPayPal != null)
            {
                doExpressChkOutPaymentReqDetails.PaymentDetails[0].ShipToAddress = newShippingAddressFromPayPal;
            }

            return doExpressChkOutPaymentReqDetails;
        }

        private void DoCompletingCart(ICart cart)
        {
            // Change status of payments to processed.
            // It must be done before execute workflow to ensure payments which should mark as processed.
            // To avoid get errors when executed workflow.
            foreach (IPayment p in cart.Forms.SelectMany(f => f.Payments).Where(p => p != null))
            {
                PaymentStatusManager.ProcessPayment(p);
            }

            if (_featureSwitch.IsSerializedCartsEnabled())
            {
                cart.AdjustInventoryOrRemoveLineItems((item, issue) => { }, _inventoryProcessor);
            }
            else
            {
                // Execute CheckOutWorkflow with parameter to ignore running process payment activity again.
                var isIgnoreProcessPayment = new Dictionary<string, object> { { "PreventProcessPayment", true } };
                OrderGroupWorkflowManager.RunWorkflow((OrderGroup)cart, OrderGroupWorkflowManager.CartCheckOutWorkflowName, true, isIgnoreProcessPayment);
            }
        }

        private IPurchaseOrder MakePurchaseOrder(DoExpressCheckoutPaymentResponseType doCheckOutResponse, ICart cart, IPayment payment)
        {
            var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            purchaseOrder.OrderNumber = payment.Properties[PayPalOrderNumberPropertyName] as string;

            // read PayPal's transactionId here, in order to store it, refund ...
            var paymentTransactionID = doCheckOutResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID;
            UpdateTransactionIdOfPaymentMethod(purchaseOrder, paymentTransactionID);

            // Update last order date time for CurrentContact
            UpdateLastOrderTimestampOfCurrentContact(CustomerContext.Current.CurrentContact, purchaseOrder.Created);

            AddNoteToPurchaseOrder(string.Empty, $"New order placed by {PrincipalInfo.CurrentPrincipal.Identity.Name} in Public site", Guid.Empty, purchaseOrder);

            // Remove old cart
            _orderRepository.Delete(cart.OrderLink);

            // Update display name of product by current language
            Utilities.UpdateDisplayNameWithCurrentLanguage(purchaseOrder);

            ((PurchaseOrder)purchaseOrder).Status = PaymentStatusCompleted;

            _orderRepository.Save(purchaseOrder);

            return purchaseOrder;
        }

        private string CreateRedirectionUrl(IPurchaseOrder purchaseOrder, string acceptUrl, string email)
        {
            var redirectionUrl = UriSupport.AddQueryString(acceptUrl, "success", "true");
            redirectionUrl = UriSupport.AddQueryString(redirectionUrl, "contactId", purchaseOrder.CustomerId.ToString());
            redirectionUrl = UriSupport.AddQueryString(redirectionUrl, "orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString());
            redirectionUrl = UriSupport.AddQueryString(redirectionUrl, "notificationMessage", string.Format(Utilities.GetLocalizationMessage("/OrderConfirmationMail/ErrorMessages/SmtpFailure"), email));
            redirectionUrl = UriSupport.AddQueryString(redirectionUrl, "email", email);

            return redirectionUrl;
        }

        /// <summary>
        /// Processes the unsuccessful transaction, delete PayPal-related information in Cookie.
        /// </summary>
        /// <param name="cancelUrl">The cancel url.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The url redirection after process.</returns>
        public string ProcessUnsuccessfulTransaction(string cancelUrl, string errorMessage)
        {
            if (HttpContext.Current == null)
            {
                return cancelUrl;
            }

            _logger.Error($"PayPal transaction failed [{errorMessage}].");
            return UriSupport.AddQueryString(cancelUrl, "message", errorMessage);
        }

        /// <summary>
        /// Captures an authorized payment.
        /// </summary>
        /// <para>
        /// See API doc here https://developer.paypal.com/webapps/developer/docs/classic/api/merchant/DoCapture_API_Operation_SOAP/
        /// </para>
        /// <param name="payment">The payment to process</param>
        /// <param name="orderGroup">The order group to process.</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        private bool ProcessPaymentCapture(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            // Implement refund feature logic for current payment gateway
            var captureAmount = payment.Amount;
            var purchaseOrder = orderGroup as IPurchaseOrder;
            if (purchaseOrder == null || captureAmount <= 0)
            {
                message = "Nothing to capture";
                return false;
            }

            var captureRequest = new DoCaptureRequestType
            {
                AuthorizationID = payment.TransactionID, // original transactionID (which PayPal gave us when DoExpressCheckoutPayment, DoDirectPayment, or CheckOut)
                Amount = _payPalAPIHelper.ToPayPalAmount(captureAmount, orderGroup.Currency), // if refund with Partial, we have to set the Amount
                CompleteType = payment.Amount >= purchaseOrder.GetTotal().Amount ? CompleteCodeType.COMPLETE : CompleteCodeType.NOTCOMPLETE,
                InvoiceID = purchaseOrder.OrderNumber
            };
            captureRequest.Note = $"[{payment.PaymentMethodName}-{payment.TransactionID}] captured {captureAmount}{captureRequest.Amount.currencyID} for [PurchaseOrder-{purchaseOrder.OrderNumber}]";

            var caller = PayPalAPIHelper.GetPayPalAPICallerServices(_paymentMethodConfiguration);
            var doCaptureReq = new DoCaptureReq { DoCaptureRequest = captureRequest };
            var captureResponse = caller.DoCapture(doCaptureReq);

            var errorCheck = _payPalAPIHelper.CheckErrors(captureResponse);
            if (!string.IsNullOrEmpty(errorCheck))
            {
                message = errorCheck;
                return false;
            }

            var captureResponseDetails = captureResponse.DoCaptureResponseDetails;
            var paymentInfo = captureResponseDetails.PaymentInfo;

            // Extract the response details.
            payment.ProviderTransactionID = paymentInfo.TransactionID;
            payment.Status = paymentInfo.PaymentStatus.ToString();

            message = $"[{payment.PaymentMethodName}] [Capture payment-{paymentInfo.TransactionID}] [Status: {paymentInfo.PaymentStatus.ToString()}] " +
                $".Response: {captureResponse.Ack.ToString()} at Timestamp={captureResponse.Timestamp.ToString()}: {paymentInfo.GrossAmount.value}{paymentInfo.GrossAmount.currencyID}";

            // add a new order note about this capture
            AddNoteToPurchaseOrder("CAPTURE", message, purchaseOrder.CustomerId, purchaseOrder);

            _orderRepository.Save(purchaseOrder);

            return true;
        }

        /// <summary>
        /// Process payment when a refund request happens.
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
        /// <param name="payment">The payment to process.</param>
        /// <param name="orderGroup">The order group to process.</param>
        /// <param name="message">The message.</param>
        /// <returns>True if refund was completed, otherwise false and set the message will make the WorkFlow activity raise PaymentExcetion(message).</returns>
        private bool ProcessPaymentRefund(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            // Implement refund feature logic for current payment gateway
            var refundAmount = payment.Amount;
            var purchaseOrder = (orderGroup as IPurchaseOrder);
            if (purchaseOrder == null || refundAmount <= 0)
            {
                message = Utilities.Translate("PayPalRefundError");
                return false;
            }

            // Call payment gateway API to do refund business
            // Create the Refund Request
            var refundRequest = new RefundTransactionRequestType
            {
                TransactionID = payment.ProviderTransactionID, // original transactionID (which payPal gave us when do ExpressCheckout)
                Memo = $"[{payment.PaymentMethodName}-{payment.TransactionID}] refunds {refundAmount}{purchaseOrder.Currency} for [PurchaseOrder-{purchaseOrder.OrderNumber}]",
                // NOTE: If RefundType is Full, do not set the amount.
                // refundRequest.RefundType = RefundType.Full; //refund a full or partial amount
                RefundType = RefundType.PARTIAL, //refund a partial amount
                Amount = _payPalAPIHelper.ToPayPalAmount(refundAmount, orderGroup.Currency) // if refund with Partial, we have to set the Amount
            };

            var caller = PayPalAPIHelper.GetPayPalAPICallerServices(_paymentMethodConfiguration);
            var refundResponse = caller.RefundTransaction(new RefundTransactionReq { RefundTransactionRequest = refundRequest });
            var errorCheck = _payPalAPIHelper.CheckErrors(refundResponse);
            if (!string.IsNullOrEmpty(errorCheck))
            {
                message = errorCheck;
                return false;
            }

            // Extract the response details.
            payment.TransactionID = refundResponse.RefundTransactionID;

            message = $"[{payment.PaymentMethodName}] [RefundTransaction-{refundResponse.RefundTransactionID}] " +
                $"Response: {refundResponse.Ack.ToString()} at Timestamp={refundResponse.Timestamp.ToString()}: {refundResponse.GrossRefundAmount.value}{refundResponse.GrossRefundAmount.currencyID}";

            // add a new order note about this refund
            AddNoteToPurchaseOrder("REFUND", message, purchaseOrder.CustomerId, purchaseOrder);

            _orderRepository.Save(purchaseOrder);

            return true;
        }

        /// <summary>
        /// Process payment when user checkout.
        /// </summary>
        /// <param name="payment">The payment to process.</param>
        /// <param name="cart">The current cart.</param>
        /// <param name="message">The message.</param>
        /// <returns>return false and set the message will make the WorkFlow activity raise PaymentExcetion(message)</returns>
        private bool ProcessPaymentCheckout(IPayment payment, ICart cart, ref string message)
        {
            var orderNumberID = _orderNumberGenerator.GenerateOrderNumber(cart);

            if (string.IsNullOrEmpty(_paymentMethodConfiguration.ExpChkoutURL) || string.IsNullOrEmpty(_paymentMethodConfiguration.PaymentAction))
            {
                message = Utilities.Translate("PayPalSettingsError");
                return false; // raise exception
            }

            var caller = PayPalAPIHelper.GetPayPalAPICallerServices(_paymentMethodConfiguration);
            var setExpressChkOutReqType = new SetExpressCheckoutRequestType(SetupExpressCheckoutReqDetailsType(cart, payment, orderNumberID));
            var setChkOutResponse = caller.SetExpressCheckout(new SetExpressCheckoutReq { SetExpressCheckoutRequest = setExpressChkOutReqType });
            var errorCheck = _payPalAPIHelper.CheckErrors(setChkOutResponse);
            if (!string.IsNullOrEmpty(errorCheck))
            {
                _logger.Error(errorCheck);
                message = string.Join("; ", setChkOutResponse.Errors.Select(e => e.LongMessage));
                return false; // raise exception
            }

            payment.Properties[PayPalOrderNumberPropertyName] = orderNumberID;
            payment.Properties[PayPalExpTokenPropertyName] = setChkOutResponse.Token;

            _orderRepository.Save(cart);

            // validation checking with PayPal OK (Server's PayPal API, Billing Address, Shipping Address, ... do redirect to PayPal.com
            var redirectUrl = CreateRedirectUrl(_paymentMethodConfiguration.ExpChkoutURL, setChkOutResponse.Token);
            message = $"---PayPal-SetExpressCheckout is successful. Redirect end user to {redirectUrl}";
            _logger.Information(message);

            payment.Properties["NextAction"] = new Action(() =>
            {
                HttpContext.Current.Response.Redirect(redirectUrl);
            });

            // If you're using workflow, uncomment the line below
            // (payment.Properties["NextAction"] as Action).Invoke();

            return true;
        }

        private SetExpressCheckoutRequestDetailsType SetupExpressCheckoutReqDetailsType(ICart cart, IPayment payment, string orderNumber)
        {
            var setExpressChkOutReqDetails = _payPalAPIHelper.CreateExpressCheckoutReqDetailsType(payment, _paymentMethodConfiguration);

            // This key is sent to PayPal using https so it is not likely it will come from other because
            // only PayPal knows this key to send back to us
            var acceptSecurityKey = Utilities.GetAcceptUrlHashValue(orderNumber);
            var cancelSecurityKey = Utilities.GetCancelUrlHashValue(orderNumber);

            _notifyUrl = UriSupport.AbsoluteUrlBySettings(Utilities.GetUrlFromStartPageReferenceProperty("PayPalPaymentPage"));

            var acceptUrl = UriSupport.AddQueryString(_notifyUrl, "accept", "true");
            acceptUrl = UriSupport.AddQueryString(acceptUrl, "hash", acceptSecurityKey);

            var cancelUrl = UriSupport.AddQueryString(_notifyUrl, "accept", "false");
            cancelUrl = UriSupport.AddQueryString(cancelUrl, "hash", cancelSecurityKey);

            setExpressChkOutReqDetails.CancelURL = cancelUrl;
            setExpressChkOutReqDetails.ReturnURL = acceptUrl;
            setExpressChkOutReqDetails.PaymentDetails = new List<PaymentDetailsType>() { _payPalAPIHelper.GetPaymentDetailsType(payment, cart, orderNumber, _notifyUrl) };

            setExpressChkOutReqDetails.BillingAddress = AddressHandling.ToAddressType(payment.BillingAddress);

            return setExpressChkOutReqDetails;
        }

        private string CreateRedirectUrl(string expChkoutURLSetting, string token)
        {
            var redirectUrl = expChkoutURLSetting;
            redirectUrl = UriSupport.AddQueryString(redirectUrl, "cmd", "_express-checkout");
            redirectUrl = UriSupport.AddQueryString(redirectUrl, "token", token);
            redirectUrl = UriSupport.AddQueryString(redirectUrl, "useraction", "commit");
            return redirectUrl;
        }

        /// <summary>
        /// Adds the note to purchase order.
        /// </summary>
        /// <param name="title">The note title.</param>
        /// <param name="detail">The note detail.</param>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="purchaseOrder">The purchase order.</param>
        private void AddNoteToPurchaseOrder(string title, string detail, Guid customerId, IPurchaseOrder purchaseOrder)
        {
            var orderNote = purchaseOrder.CreateOrderNote();
            orderNote.Type = OrderNoteTypes.System.ToString();
            orderNote.CustomerId = customerId != Guid.Empty ? customerId : PrincipalInfo.CurrentPrincipal.GetContactId();
            orderNote.Title = !string.IsNullOrEmpty(title) ? title : detail.Substring(0, Math.Min(detail.Length, 24)) + "...";
            orderNote.Detail = detail;
            orderNote.Created = DateTime.UtcNow;
            purchaseOrder.Notes.Add(orderNote);
        }

        /// <summary>
        /// Loop through all payments in the PurchaseOrder, find payment of paymentMethod type, set TransactionId.
        /// </summary>
        /// <param name="purchaseOrder">The purchase order.</param>
        /// <param name="paymentGatewayTransactionID">The transactionId from PayPal.</param>
        private void UpdateTransactionIdOfPaymentMethod(IPurchaseOrder purchaseOrder, string paymentGatewayTransactionID)
        {
            // loop through all payments in the PurchaseOrder, find payment with id equal guidPaymentMethodId, set TransactionId
            foreach (var payment in purchaseOrder.Forms.SelectMany(form => form.Payments).Where(payment => payment.PaymentMethodId.Equals(_paymentMethodConfiguration.PaymentMethodId)))
            {
                payment.TransactionID = paymentGatewayTransactionID;
                payment.ProviderTransactionID = paymentGatewayTransactionID;
            }
        }

        /// <summary>
        /// Update last order time stamp which current user completed.
        /// </summary>
        /// <param name="contact">The customer contact.</param>
        /// <param name="datetime">The order time.</param>
        private void UpdateLastOrderTimestampOfCurrentContact(CustomerContact contact, DateTime datetime)
        {
            if (contact != null)
            {
                contact.LastOrder = datetime;
                contact.SaveChanges();
            }
        }
    }
}

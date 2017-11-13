using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using Geta.EPi.Commerce.Payments.Klarna.Checkout.Extensions;
using Geta.Klarna.Checkout;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Plugins.Payment;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.Klarna.Checkout.Models;
using OrderStatus = Mediachase.Commerce.Orders.OrderStatus;

namespace Geta.EPi.Commerce.Payments.Klarna.Checkout
{
    public class KlarnaCheckoutPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        private static Injected<IPostProcessPayment> _postProcessPayment;
        private static Injected<IOrderRepository> _orderRepository;
        private static IPostProcessPayment PostProcessPayment
        {
            get { return _postProcessPayment.Service; }
        }

        private static readonly ILogger Logger = LogManager.GetLogger(typeof(KlarnaCheckoutPaymentGateway));

        public override bool ProcessPayment(Payment payment, ref string message)
        {
            var paymentProcessingResult = ProcessPayment(payment.Parent.Parent, payment);
            message = paymentProcessingResult.Message;
            return paymentProcessingResult.IsSuccessful;
        }

        private string GetInvoiceId(IPurchaseOrder purchaseOrder)
        {
            string invoiceId = (string)purchaseOrder.Properties[MetadataConstants.InvoiceId];
            if (string.IsNullOrEmpty(invoiceId))
            {
                return (string)purchaseOrder.Properties[MetadataConstants.InvoiceNumber];
            }
            return invoiceId;
        }

        private string GetReservation(IPayment payment)
        {
            string reservationId = (string)payment.Properties[MetadataConstants.ReservationId];
            if (string.IsNullOrEmpty(reservationId))
            {
                return (string)payment.Properties[MetadataConstants.ReservationField];
            }
            return reservationId;
        }

        private ProviderSettings _klarnaSettings;
        internal ProviderSettings KlarnaSettings
        {
            get
            {
                if (_klarnaSettings == null)
                {
                    _klarnaSettings = new ProviderSettings(
                        bool.Parse(Settings[KlarnaConstants.IsProduction]),
                        bool.Parse(Settings[KlarnaConstants.NewsletterDefaultChecked]),
                        Settings[KlarnaConstants.MerchantId],
                        Settings[KlarnaConstants.Secret],
                        Settings[KlarnaConstants.Locale]);
                }
                Logger.Debug(string.Format("Active Klarna merchant id is {0}. Is testing environment: {1}", _klarnaSettings.MerchantId, !_klarnaSettings.IsProduction));
                return _klarnaSettings;
            }
        }

        private void VerifyConfiguration()
        {
            if (string.IsNullOrEmpty(Settings[KlarnaConstants.MerchantId]))
            {
                throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "",
                    "Payment configuration is not valid. Missing payment provider merchant identification nr.");
            }

            if (string.IsNullOrEmpty(Settings[KlarnaConstants.Secret]))
            {
                throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "",
                    "Payment configuration is not valid. Missing payment provider merchant secret.");
            }

            if (string.IsNullOrEmpty(Settings[KlarnaConstants.Locale]))
            {
                throw new PaymentException(PaymentException.ErrorType.ConfigurationError, "",
                    "Payment method configuration is not valid. Missing payment Locale.");
            }

            Logger.Debug("Payment method configuration verified.");
        }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            Logger.Debug("Klarna checkout gateway. Processing Payment ....");
            VerifyConfiguration();

            var transactionType = payment.TransactionType.ToUpper();

            var orderApiClient = new OrderApiClient(Int32.Parse(KlarnaSettings.MerchantId), KlarnaSettings.Secret, KlarnaSettings.CurrentLocale, KlarnaSettings.IsProduction, KlarnaSettings.NewsletterDefaultChecked);

            try
            {
                switch (transactionType)
                {
                    case "CAPTURE":
                        {
                            var reservation = GetReservation(payment);

                            if (string.IsNullOrEmpty(reservation))
                            {
                                var errorMessage =
                                    "CAPTURE operation KlarnaCheckoutPaymentGateway failed. Metafield 'Reservation' on KlarnaPayment is empty.";
                                Logger.Error(errorMessage);
                                throw new Exception(errorMessage);
                            }

                            var orderForm = orderGroup.GetFirstForm();

                            // We will include cart items (in case of partial shipment)
                            var shipment = orderGroup.GetFirstShipment();  // only 1 shipment is valid
                            var cartItems = orderForm.GetAllLineItems().Select(item => item.ToCartItem(true)).ToList();

                            var shippingPromotion = orderForm.Promotions.FirstOrDefault(p => p.DiscountType == DiscountType.Shipping);
                            cartItems.AddRange(shipment.ToCartItems(shippingPromotion, orderGroup.Currency, true));

                            var purchaseOrder = orderGroup as IPurchaseOrder;

                            if (purchaseOrder == null)
                            {
                                var errorMessage =
                                    "CAPTURE operation KlarnaCheckoutPaymentGateway failed. Uanble to cast orderGroup to PurchaseOrder.";
                                Logger.Error(errorMessage);
                                throw new Exception(errorMessage);
                            }


                            var trackingNr = (string)purchaseOrder.Properties["TrackingNumber"];

                            string infoMsg = $"KlarnaCheckoutPaymentGateway: Activating reservation {reservation}. Transaction id: {payment.TransactionID}. Tracking number: {trackingNr}.";
                            Logger.Debug(infoMsg);

                            var response = orderApiClient.Activate(reservation, payment.TransactionID, trackingNr, cartItems);
                            payment.Status = response.IsSuccess ? PaymentStatus.Processed.ToString() : PaymentStatus.Failed.ToString();
                            if (response.IsSuccess)
                            {
                                orderGroup.Notes.Add(new OrderNote
                                {
                                    Title = "Invoice number",
                                    Detail = response.InvoiceNumber
                                });

                                // we need to save invoice number incase of refunds later
                                purchaseOrder.Properties[MetadataConstants.InvoiceId] = response.InvoiceNumber;
                                _orderRepository.Service.Save(purchaseOrder);

                                PostProcessPayment.PostCapture(response, payment);
                                return PaymentProcessingResult.CreateSuccessfulResult(string.Empty);
                            }

                            PostProcessPayment.PostCapture(response, payment);
                            Logger.Error(string.Format("Capture failed for order {0} with reservation {1}. Error message: {2}", trackingNr, reservation, response.ErrorMessage));
                            throw new Exception(response.ErrorMessage);
                        }
                    case "VOID":
                        {
                            var reservation = GetReservation(payment);

                            if (string.IsNullOrEmpty(reservation))
                            {
                                var errorMessage =
                                    "VOID operation KlarnaCheckoutPaymentGateway failed. Metafield 'Reservation' on KlarnaPayment is empty.";
                                Logger.Error(errorMessage);
                                throw new Exception(errorMessage);
                            }

                            Logger.Debug($"Cancel reservation called with reservation {reservation}. Transaction id is {payment.TransactionID}.");

                            var cancelResult = orderApiClient.CancelReservation(reservation);
                            if (cancelResult.IsSuccess)
                            {
                                orderGroup.OrderStatus = OrderStatus.Cancelled;
                                payment.Status = PaymentStatus.Processed.ToString();
                            }
                            else
                            {
                                payment.Status = PaymentStatus.Failed.ToString();
                            }

                            _orderRepository.Service.Save(orderGroup);

                            PostProcessPayment.PostAnnul(cancelResult.IsSuccess, payment);

                            if (cancelResult.IsSuccess == false)
                            {
                                var errorMessage = $"VOID operation KlarnaCheckoutPaymentGateway failed. Error is {cancelResult.ErrorMessage}.";
                                Logger.Error(errorMessage);
                                throw new Exception(cancelResult.ErrorMessage);
                            }

                            return cancelResult.IsSuccess ? PaymentProcessingResult.CreateSuccessfulResult(string.Empty) : PaymentProcessingResult.CreateUnsuccessfulResult(string.Empty);

                        }
                    case "CREDIT":
                        {
                            var purchaseOrder = orderGroup as PurchaseOrder;
                            if (purchaseOrder != null)
                            {
                                var invoiceNumber = GetInvoiceId(purchaseOrder);

                                if (string.IsNullOrEmpty(invoiceNumber))
                                {
                                    var errorMessage =
                                        "CREDIT operation on KlarnaCheckoutPaymentGateway failed. Metafield 'InvoiceNumber' is empty.";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                var returnFormToProcess = purchaseOrder.ReturnOrderForms.FirstOrDefault(p => p.Status == ReturnFormStatus.AwaitingCompletion.ToString() && p.Total == payment.Amount);

                                if (returnFormToProcess == null)
                                {
                                    payment.Status = PaymentStatus.Failed.ToString();
                                    PostProcessPayment.PostCredit(new RefundResponse() { IsSuccess = false, ErrorMessage = "No return forms to process." }, payment);
                                    return PaymentProcessingResult.CreateUnsuccessfulResult(string.Empty);
                                }

                                // Determine if this is full refund, in that case we will call CreditInvoice
                                // If payment.Amount = captured amount then do full refund
                                var orderForm = orderGroup.GetFirstForm();
                                var capturedAmount = orderForm.Payments
                                    .Where(p => p.TransactionType == "Capture" & p.Status == PaymentStatus.Processed.ToString())
                                    .Sum(p => p.Amount);

                                var result = new RefundResponse();

                                if (capturedAmount == payment.Amount) // full refund
                                {
                                    result = orderApiClient.CreditInvoice(invoiceNumber);
                                }
                                else
                                {
                                    var returnItems = returnFormToProcess.LineItems.Select(item => item.ToCartItem()).ToList();
                                    // if shipment is part of returnForm, then we will return shipping cost as well
                                    var shipment = returnFormToProcess.Shipments[0];
                                    if (shipment != null && shipment.ShippingTotal > 0)
                                    {
                                        var shippingPromotion = orderForm.Promotions.FirstOrDefault(p => p.DiscountType == DiscountType.Shipping);
                                        returnItems.AddRange(shipment.ToCartItems(shippingPromotion, orderGroup.Currency));
                                    }

                                    result = orderApiClient.HandleRefund(invoiceNumber, returnItems);
                                }

                                payment.Status = result.IsSuccess ? PaymentStatus.Processed.ToString() : PaymentStatus.Failed.ToString();
                                _orderRepository.Service.Save(purchaseOrder);

                                PostProcessPayment.PostCredit(result, payment);

                                if (result.IsSuccess == false)
                                {
                                    Logger.Error(result.ErrorMessage);
                                    throw new Exception(result.ErrorMessage);
                                }
                                return PaymentProcessingResult.CreateSuccessfulResult(string.Empty);
                            }
                            return PaymentProcessingResult.CreateUnsuccessfulResult(string.Empty);
                        }
                }

            }
            catch (Exception exception)
            {
                Logger.Error("Process payment failed with error: " + exception.Message, exception);
                throw;
            }

            return PaymentProcessingResult.CreateUnsuccessfulResult(string.Empty); ;
        }
    }
}

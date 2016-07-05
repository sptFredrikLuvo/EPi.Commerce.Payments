using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Security;
using Mediachase.BusinessFoundation;
using Mediachase.Commerce;
using Mediachase.Commerce.Manager.Apps.Common.Design;
using Mediachase.Commerce.Manager.Apps.Core.Controls;
using Mediachase.Commerce.Manager.Apps_Code.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;
using Mediachase.MetaDataPlus;
using Mediachase.Web.Console.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BlockHeaderLight = Mediachase.Commerce.Manager.Apps.Common.Design.BlockHeaderLight;

namespace Mediachase.Commerce.Manager.Apps.Order.Modules
{
    public class TestShipmentSummary : MCDataBoundControl
    {
        private const string _OrderFormIdParamKey = "OrderFormId";

        private const string _ShipmentIdParamKey = "ShipmentId";

        private const string _CreateReturnCommandName = "cmdOrderCreateEditReturn";

        private OrderGroup _orderGroup;

        protected BlockHeaderLight bhl;

        protected Label Label4;

        protected Label lblItemsSubTotal;

        protected Label Label5;

        protected Label lblOrderLevelDiscount;

        protected Label Label9;

        protected Label lblOrderSubTotal;

        protected Label Label7;

        protected Label lblShippingCost;

        protected Label Label2;

        protected Label lblShippingDiscount;

        protected Label Label3;

        protected Label lblShippingTax;

        protected Label Label1;

        protected Label lblShipTotal;

        protected BlockHeaderLight bh2;

        protected Label Label8;

        protected Label lblShipStatus;

        protected ButtonsHolder btnShipmentStatus;

        protected BlockHeaderLight bh3;

        protected ButtonsHolder CompletePickupButton;

        protected BlockHeaderLight BlockHeaderLight1;

        protected ButtonsHolder ReturnExchangeButtonsHolder;

        private int _shipmentId
        {
            get
            {
                if (this.ViewState["__ShipmentId"] == null)
                {
                    return -1;
                }
                return (int)this.ViewState["__ShipmentId"];
            }
            set
            {
                this.ViewState["__ShipmentId"] = value;
            }
        }

        protected OrderGroup CurrentOrderGroup
        {
            get
            {
                OrderGroup newOrderGroupObject = this._orderGroup;
                if (newOrderGroupObject == null)
                {
                    if (this.OrderGroupId <= 0)
                    {
                        //newOrderGroupObject = OrderSessionManager.GetNewOrderGroupObject(this.OrderGroupId, 0);
                    }
                    else if (!base.Request.QueryString["_v"].Equals("PaymentPlan-ObjectView"))
                    {
                        newOrderGroupObject = OrderHelper.GetPurchaseOrderById(this.OrderGroupId);
                    }
                    else
                    {
                        newOrderGroupObject = OrderHelper.GetPaymentPlanById(this.CustomerId, this.OrderGroupId);
                    }
                    this._orderGroup = newOrderGroupObject;
                }
                return newOrderGroupObject;
            }
        }

        public Guid CustomerId
        {
            get
            {
                return ManagementHelper.GetGuidFromQueryString("customerid");
            }
        }

        protected int OrderGroupId
        {
            get
            {
                object item = HttpContext.Current.Items["OrderGroupId"] ?? ManagementHelper.GetIntFromQueryString("id");
                return (int)item;
            }
        }

        public TestShipmentSummary()
        {
        }

        public override void DataBind()
        {
            OrderGroup paymentPlanById;
            if (base.Request.QueryString["_v"].Equals("PaymentPlan-ObjectView"))
            {
                paymentPlanById = OrderHelper.GetPaymentPlanById(PrincipalInfo.CurrentPrincipal.GetContactId(), this.OrderGroupId);
            }
            else
            {
                paymentPlanById = OrderHelper.GetPurchaseOrderById(this.OrderGroupId);
            }
            OrderGroup orderGroup = paymentPlanById;
            Shipment dataItem = null;
            ObjectRepeaterDataItem objectRepeaterDataItem = this.DataItem as ObjectRepeaterDataItem;
            if (objectRepeaterDataItem != null && objectRepeaterDataItem.DataItem != null)
            {
                dataItem = (Shipment)objectRepeaterDataItem.DataItem;
                this._shipmentId = dataItem.Id;
                this.btnShipmentStatus.ContainerId = dataItem.Id.ToString();
                this.btnShipmentStatus.DataBind();
                this.ReturnExchangeButtonsHolder.ContainerId = dataItem.Id.ToString();
                this.ReturnExchangeButtonsHolder.DataBind();
                this.CompletePickupButton.ContainerId = dataItem.Id.ToString();
                this.CompletePickupButton.DataBind();
            }
            if (dataItem != null)
            {
                OrderShipmentStatus orderShipmentStatus = OrderStatusManager.GetOrderShipmentStatus(dataItem);
                this.lblShipStatus.Text = (string)base.GetGlobalResourceObject("OrderStrings", string.Concat(typeof(OrderShipmentStatus).Name, "_", orderShipmentStatus.ToString()));
                Currency currency = new Currency(orderGroup.BillingCurrency);
                decimal num = ((IEnumerable<ILineItem>)dataItem.LineItems.ToArray<ILineItem>()).Sum<ILineItem>((ILineItem x) => x.TryGetDiscountValue((ILineItemDiscountAmount y) => y.OrderAmount));
                Label str = this.lblItemsSubTotal;
                Money money = new Money(dataItem.SubTotal + num, currency);
                str.Text = money.ToString();
                Label label = this.lblOrderLevelDiscount;
                money = new Money(num, currency);
                label.Text = money.ToString();
                Label str1 = this.lblOrderSubTotal;
                money = new Money(dataItem.SubTotal, currency);
                str1.Text = money.ToString();
                Label label1 = this.lblShippingCost;
                money = new Money(dataItem.ShippingSubTotal, currency);
                label1.Text = money.ToString();
                Label str2 = this.lblShippingDiscount;
                money = new Money(dataItem.ShippingDiscountAmount, currency);
                str2.Text = money.ToString();
                Label label2 = this.lblShippingTax;
                money = new Money(dataItem.ShippingTax, currency);
                label2.Text = money.ToString();
                Label str3 = this.lblShipTotal;
                money = new Money(((dataItem.SubTotal + dataItem.ShippingSubTotal) + dataItem.ShippingTax) - dataItem.ShippingDiscountAmount, currency);
                str3.Text = money.ToString();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string str = "PurchaseOrder-ObjectView";
            if (base.Request.QueryString["_v"].Equals("PaymentPlan-ObjectView"))
            {
                str = "PaymentPlan-ObjectView";
            }
            this.btnShipmentStatus.ViewName = str;
            this.btnShipmentStatus.ClassName = "OrderShipment";
            this.btnShipmentStatus.PlaceName = "StatusChanger";
            this.btnShipmentStatus.HolderMode = ButtonsHolder.Mode.ListViewUI;
            this.btnShipmentStatus.Direction = RepeatDirection.Vertical;
            this.ReturnExchangeButtonsHolder.ViewName = str;
            this.ReturnExchangeButtonsHolder.ClassName = "OrderShipment";
            this.ReturnExchangeButtonsHolder.PlaceName = "OrderView";
            this.ReturnExchangeButtonsHolder.HolderMode = ButtonsHolder.Mode.ListViewUI;
            this.ReturnExchangeButtonsHolder.Direction = RepeatDirection.Vertical;
            this.CompletePickupButton.ViewName = str;
            this.CompletePickupButton.ClassName = "OrderShipment";
            this.CompletePickupButton.PlaceName = "CompletePickup";
            this.CompletePickupButton.HolderMode = ButtonsHolder.Mode.ListViewUI;
            this.CompletePickupButton.Direction = RepeatDirection.Vertical;
        }
    }
}
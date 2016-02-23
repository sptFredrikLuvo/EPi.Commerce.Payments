using System;

namespace Geta.Klarna.Checkout.Models
{
    public class MerchantReference
    {
        public static MerchantReference Empty = new EmptyMerchantReference();
        public virtual bool IsEmpty { get { return false; } }

        public string OrderId1 { get; private set; }
        public string OrderId2 { get; private set; }

        public MerchantReference(string orderId1, string orderId2 = "")
        {
            if (orderId1 == null) throw new ArgumentNullException("orderId1");
            if (orderId2 == null) throw new ArgumentNullException("orderId2");
            OrderId1 = orderId1;
            OrderId2 = orderId2;
        }

        private class EmptyMerchantReference : MerchantReference 
        {
            public EmptyMerchantReference() 
                : base("", "") { }

            public override bool IsEmpty
            {
                get { return true; }
            }
        }
    }
}
using System;
using System.Runtime.Serialization;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Netaxept.Checkout.Business
{
    [Serializable]
    public class NetaxeptPayment : OtherPayment
    {

        public NetaxeptPayment()
        {
            
        }

        public NetaxeptPayment(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

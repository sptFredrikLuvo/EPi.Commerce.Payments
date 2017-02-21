using System;
using System.Runtime.Serialization;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus.Configurator;

namespace Geta.Commerce.Payments.PayPal
{
    /// <summary>
    /// Represents Payment class for PayPal.
    /// </summary>
    [Serializable]
    public class PayPalPayment : Mediachase.Commerce.Orders.Payment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayPalPayment"/> class.
        /// </summary>
        public PayPalPayment()
            : base(PayPalPaymentMetaClass)
        {
            this.PaymentType = Mediachase.Commerce.Orders.PaymentType.Other;
            ImplementationClass = this.GetType().AssemblyQualifiedName; // need to have assembly name in order to retreive the correct type in ClassInfo
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayPalPayment"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected PayPalPayment(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        {
            this.PaymentType = Mediachase.Commerce.Orders.PaymentType.Other;
            ImplementationClass = this.GetType().AssemblyQualifiedName; // need to have assembly name in order to retreive the correct type in ClassInfo
        }

        private static MetaClass _MetaClass;
        /// <summary>
        /// Gets the credit card payment meta class.
        /// </summary>
        /// <value>The credit card payment meta class.</value>
        public static MetaClass PayPalPaymentMetaClass
        {
            get
            {
                if (_MetaClass == null)
                {
                    _MetaClass = MetaClass.Load(OrderContext.MetaDataContext, "PayPalPayment");
                }

                return _MetaClass;
            }
        }

        /// <summary>
        /// Represents the PayPal order number
        /// </summary>
        public string PayPalOrderNumber
        {
            get { return GetString("PayPalOrderNumber"); }
            set { this["PayPalOrderNumber"] = value; }
        }

        /// <summary>
        /// Represents the PayPal exp token
        /// </summary>
        public string PayPalExpToken
        {
            get { return GetString("PayPalExpToken"); }
            set { this["PayPalExpToken"] = value; }
        }
    }
}

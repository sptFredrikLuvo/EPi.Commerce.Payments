using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geta.Klarna.Checkout.Models
{
    public class CaptureHandler
    {
        public virtual void HandleCapture(ActivateResponse activateResult)
        {
            // override this method to do more on capture payment
        }
    }
}

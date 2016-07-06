using System;

namespace Geta.Netaxept.Checkout.Models
{
    /// <summary>
    /// Client connection 
    /// </summary>
    public class ClientConnection
    {
        public string MerchantId { get; private set; }
        public string Token { get; private set; }

        public ClientConnection(string merchantId, string token)
        {
            if (string.IsNullOrEmpty(merchantId))
            {
                throw new ArgumentNullException(nameof(merchantId));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            MerchantId = merchantId;
            Token = token;
        }
    }
}

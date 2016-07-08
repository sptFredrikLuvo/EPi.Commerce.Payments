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
        public bool IsProduction { get; private set; }

        /// <summary>
        /// Public constructor, set the merchartId and token
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="token"></param>
        /// <param name="isProduction"></param>
        public ClientConnection(string merchantId, string token, bool isProduction)
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
            IsProduction = isProduction;
        }
    }
}

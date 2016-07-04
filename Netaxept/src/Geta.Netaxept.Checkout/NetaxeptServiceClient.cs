﻿
using System.Configuration;
using Geta.Netaxept.Checkout.NetaxeptWebServiceClient;

namespace Geta.Netaxept.Checkout
{
    public class NetaxeptServiceClient
    {
        private readonly NetaxeptClient _client;

        /// <summary>
        /// Public constructor
        /// </summary>
        public NetaxeptServiceClient()
        {
            _client = new NetaxeptClient();
        }

        public void Register(string mercharntId, string token )
        {
            var response = _client.Register(mercharntId, token, new RegisterRequest
            {
                Terminal = new Terminal
                {
                    OrderDescription = "Just a test transaction",
                    RedirectUrl = "http://netaxept.localtest.me/"
                },
                Order = new Order
                {
                    Amount = "39500",
                    CurrencyCode = "NOK",
                    OrderNumber = "12345"
                },
                Environment = new Environment
                {
                    WebServicePlatform = "WCF"
                },
                Recurring = new Recurring
                {
                    Type = "S"
                }
            });
        }
    }
}
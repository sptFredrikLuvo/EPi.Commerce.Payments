using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Netaxept.Checkout;
using Xunit;

namespace Test.Integration
{
    public class NetaxeptSerivceClientTest
    {
        [Fact]
        public void Test()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var client = new NetaxeptServiceClient();
            //client.Register(appSettings["Netaxept:MerchantId"], appSettings["Netaxept:Token"], "http://localhost/thanks");
        }
    }
}

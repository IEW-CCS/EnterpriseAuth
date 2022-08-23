using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    public class ClientVerifyRequest
    {
        public string tokenID { get; set; }
        public string clientID { get; set; }

        public ClientVerifyRequest()
        {
            this.tokenID = "";
            this.clientID = "";
        }
    }
}

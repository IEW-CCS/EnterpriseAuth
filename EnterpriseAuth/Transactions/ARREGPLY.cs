using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class ARREGPLY
    {
        public string ServerName { get; set; }
        public string HttpServiceURL { get; set; }
        public string WSServiceURL { get; set; }
        public string HttpToken { get; set; }
        public string ServerRSAPublicKey { get; set; }

        public ARREGPLY()
        {
            ServerName = string.Empty;
            HttpServiceURL = string.Empty;
            WSServiceURL = string.Empty;
            HttpToken = string.Empty;
            ServerRSAPublicKey = string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    public class ClientNotify
    {
        public string processStep { get; set; }
        public string serviceUUID { get; set; }
        public string result { get; set; }

        public ClientNotify()
        {
            this.processStep = "";
            this.serviceUUID = "";
            this.result = "";
        }
    }
}

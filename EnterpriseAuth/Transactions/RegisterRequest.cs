using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    public class RegisterRequest_tmp

    {
        public string serverURL { get; set; }
        public string userName { get; set; }
        public string userPassword { get; set; }
        public string publicKey { get; set; }
        public string blueToothMac { get; set; }
        public string processStep { get; set; }

        public RegisterRequest_tmp()
        {
            this.serverURL = "";
            this.userName = "";
            this.userPassword = "";
            this.publicKey = "";
            this.blueToothMac = "";
            this.processStep = "";
        }
    }
}

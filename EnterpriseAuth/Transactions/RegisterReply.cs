using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    public class RegisterReply
    {
        public string serverName { get; set; }
        public string httpURL { get; set; }
        public string webSocketURL { get; set; }
        public string clientID { get; set; }
        public string credentialHash { get; set; }
        public string tokenID { get; set; }
        public string serverPublicKey { get; set; }
        public string processStep { get; set; }

        public RegisterReply()
        {
            this.serverName = "";
            this.httpURL = "";
            this.webSocketURL = "";
            this.clientID = "";
            this.credentialHash = "";
            this.tokenID = "";
            this.serverPublicKey = "";
            this.processStep = "";
        }
    }
}

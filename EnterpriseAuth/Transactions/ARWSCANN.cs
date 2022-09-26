using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class ARWSCANN
    {
        public string Credential { get; set; }
        public string CredentialSign { get; set; }
        public string SignedPublicKey { get; set; }
        public string DeviceUUID { get; set; }

        public ARWSCANN()
        {
            Credential = string.Empty;
            CredentialSign = string.Empty;
            SignedPublicKey = string.Empty;
            DeviceUUID = string.Empty;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    public class Credential
    {
        public string CredContent { get; set; }
        public string CredSign { get; set; }
        public Credential()
        {
            CredContent = string.Empty;
            CredSign = string.Empty;
        }
    }
    class CREDINFO
    {
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string APPGuid { get; set; }
        public string APPVersion { get; set; }
        public string DeviceUUID { get; set; }
        public int Nonce { get; set; }
        public DateTime CreateDateTime { get; set; }

        public CREDINFO()
        {
            ServerName = string.Empty;
            UserName = string.Empty;
            APPGuid = string.Empty;
            APPVersion = string.Empty;
            DeviceUUID = string.Empty;
            Nonce = 0;
        }
    }
}

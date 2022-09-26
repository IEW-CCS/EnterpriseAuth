using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class CRCRLPLY
    {
        public string ServerName { get; set; }
        public string Credential { get; set; }
        public DateTime TimeStamp { get; set; }

        public CRCRLPLY()
        {
            ServerName = string.Empty;
            Credential = string.Empty;
            TimeStamp = DateTime.Now;
        }
    }
}

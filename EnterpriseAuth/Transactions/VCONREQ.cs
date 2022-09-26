using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class VCONREQ
    {
        public string DeviceCodeHash { get; set; }

        public VCONREQ()
        {
            DeviceCodeHash = "";
        }
    }
}

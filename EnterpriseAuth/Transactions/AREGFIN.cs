using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class AREGFIN
    {
        public string AuthenticationToken { get; set; }
        public string AuthenticationURL { get; set; }

        public AREGFIN()
        {
            this.AuthenticationToken = "";
            this.AuthenticationURL = "";
        }
    }
}

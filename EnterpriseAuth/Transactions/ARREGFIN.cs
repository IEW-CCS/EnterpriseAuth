using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class ARREGFIN
    {
        public string AuthenticationToken { get; set; }
        public string AuthenticationURL { get; set; }

        public ARREGFIN()
        {
            this.AuthenticationToken = "";
            this.AuthenticationURL = "";
        }
    }
}

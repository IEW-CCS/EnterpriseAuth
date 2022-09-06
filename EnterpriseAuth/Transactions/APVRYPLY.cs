using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class APVRYPLY
    {
        public string HashPassword { get; set; }
        public DateTime TimeStamp { get; set; }

        public APVRYPLY()
        {
            HashPassword = string.Empty;
            TimeStamp = DateTime.Now;
        }
    }
}

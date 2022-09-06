using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class APVRYCMP
    {
        public string UserName { get; set; }
        public string Result { get; set; }
        public DateTime TimeStamp { get; set; }

        public APVRYCMP()
        {
            UserName = string.Empty;
            Result = string.Empty;
            TimeStamp = DateTime.Now;
        }
    }
}

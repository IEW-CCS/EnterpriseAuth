﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class CCREDREQ
    {
        public string UserName { get; set; }
        public DateTime TimeStamp { get; set; }

        public CCREDREQ()
        {
            UserName = string.Empty;
            TimeStamp = DateTime.Now;
        }
    }
}

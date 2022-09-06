using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class WSTrx
    {
        public string ProcStep { get; set; }
        public int ReturnCode { get; set; }
        public string ReturnMsg { get; set; }
        public string DataContent { get; set; }
        public string ECS { get; set; }            // Encrype with Public Key 
        public string ECSSign { get; set; }

        public WSTrx()
        {
            ProcStep = string.Empty;
            ReturnCode = 0;
            ReturnMsg = string.Empty;
            DataContent = string.Empty;
            ECS = string.Empty;
            ECSSign = string.Empty;
        }
    }
}

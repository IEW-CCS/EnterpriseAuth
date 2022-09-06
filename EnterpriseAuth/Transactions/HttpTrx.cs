using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class HttpTrx
    {
        public string username { get; set; }
        public string devicetype { get; set; }
        public string procstep { get; set; }
        public int returncode { get; set; }
        public string returnmsg { get; set; }
        public string datacontent { get; set; }
        public string ecs { get; set; }            // Encrype with Public Key 
        public string ecssign { get; set; }

        public HttpTrx()
        {
            username = string.Empty;
            devicetype = string.Empty;
            procstep = string.Empty;
            returncode = 0;
            //returncode = string.Empty;
            returnmsg = string.Empty;
            datacontent = string.Empty;
            ecs = string.Empty;
            ecssign = string.Empty;
        }
    }
}

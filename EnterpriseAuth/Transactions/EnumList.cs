using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    public static class EnumList
    {
       public  enum ProcessStep
        {
            AREG_REQ,        //App Regist Request
            AREG_PLY,        //App Regist Reply
            UUID_RPT,        // UUID Report 
            UUID_ACK,        // UUID Report ACK
            CRED_REQ,        //credential Request
            CRED_PLY,        //credential Reply
            AREG_CMP,        //App Regist Complete
            AREG_FIN,        //App Regist Finished

            VCON_REQ,      // Verify Connect Request
            VCON_PLY,        // Verift Connect Reply
            AVRY_REQ,       // App Vryope Request
            AVRY_PLY,         // App Vryope Reply
            AHPW_REQ,      // App Hash Password Request
            AHPW_PLY,        // App Hash Password Reply

            WCON_REQ,      // WebSocket Connect Request
            WCON_PLY,        // WebSocket Connect Request
            WUID_ANN,        // WebSocket UUID Announce 
            STEP_ERR
        }


        public enum DeviceType
        {
            CONSOLE,
            MOBILE
        }

    }
}

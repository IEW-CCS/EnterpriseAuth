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
            CRED_REQ,        //credential Request
            CRED_PLY,        //credential Reply
            UUID_RPT,        // UUID Report 
            UUID_ACK,        // UUID Report ACK
            UUID_ANN,        // UUID Announce
            AREG_CMP,        //App Regist Complete
            AREG_FIN,        //App Regist Finished

            AVRY_REQ,        // App Vryope Request
            AVRY_PLY,        // App Vryope Reply
            AVRY_CMP,        // App Vryope Complete
            AVRY_FIN,        // App Vryope Finished

            WSKT_CON,       // WebSocket Connect
            STEP_ERR
        }


        public enum DeviceType
        {
            CONSOLE,
            MOBILE


        }

    }
}

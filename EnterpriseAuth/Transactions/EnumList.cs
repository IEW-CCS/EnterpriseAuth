﻿using System;
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
            /********* Naming Rule Modify *********
               104-1    第一碼 Ａ   104-2 第一碼 C
               //--  Register 第二碼 R  --
                ARREGREQ      App Reg REQ
                ARREGPLY      App Reg PLY
                ARWSCREQ      App Reg WS Connect REQ
                ARWSCPLY      App Reg WS Connect PLY
                CRUIDRPT      Mobile Reg UID Report
                CRUIDPLY      Mobile Reg UID Reply
                CRCRLREQ      Mobile Reg Credential Request
                CRCRLPLY      Mobile Reg Credential Reply
                ARWSCANN      App Reg WS Credential Announce
                ARREGCMP      App Reg Complete 
                ARREGFIN      App Reg Finished

               //-- Authentication 第二碼 Ａ  --
                AACONREQ      App Auth connect REQ
                AACONPLY      App Auth connect PLY
                AAUTHREQ      App Auth REQ
                AAUTHPLY      App Auth PLY
                AAPSWREQ      App PassWord REQ
                AAPSWPLY      App PassWord PLY
    
             ******** Naming Rule Modify **********/

            ARREGREQ,
            ARREGPLY,
            ARWSCREQ,
            ARWSCPLY,
            CRUIDRPT,
            CRUIDPLY,
            CRCRLREQ,
            CRCRLPLY,
            ARWSCANN,
            ARREGCMP,
            ARREGFIN,

            AACONREQ,
            AACONPLY,
            AAUTHREQ,
            AAUTHPLY,
            AAPSWREQ,
            AAPSWPLY,

            STEP_ERR
        }


        public enum DeviceType
        {
            CONSOLE,
            MOBILE
        }

    }
}

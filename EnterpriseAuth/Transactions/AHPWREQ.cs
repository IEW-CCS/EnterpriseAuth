﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.Transactions
{
    class AHPWREQ
    {
        public string BiometricsResult { get; set; }
        public string SerialNumber { get; set; }
        public string CredentialSign { get; set; }

        public AHPWREQ()
        {
            this.BiometricsResult = "";
            this.SerialNumber = "";
            this.CredentialSign = "";
        }
    }
}

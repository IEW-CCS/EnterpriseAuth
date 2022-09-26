using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace EnterpriseAuth.Transactions
{
    class DeserializeObj
    {
        public static HttpTrx _HttpTrx(string DataContent)
        {
            HttpTrx obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<HttpTrx>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;

        }
        public static ECS _ECS(string DataContent)
        {
            ECS obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<ECS>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static APREGREQ _APREGREQ(string DataContent)
        {
            APREGREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<APREGREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static APREGPLY _APREGPLY(string DataContent)
        {
            APREGPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<APREGPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }

        public static AREGFIN _AREGFIN(string DataContent)
        {
            AREGFIN obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AREGFIN>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }


        public static VCONREQ _VCONREQ(string DataContent)
        {
            VCONREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<VCONREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }

        public static VCONPLY _VCONPLY(string DataContent)
        {
            VCONPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<VCONPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }

        public static AHPWREQ _AHPWREQ(string DataContent)
        {
            AHPWREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AHPWREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }

        public static AHPWPLY _AHPWPLY(string DataContent)
        {
            AHPWPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AHPWPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }


        public static APREGCMP _APREGCMP(string DataContent)
        {
            APREGCMP obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<APREGCMP>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static CCREDREQ _CCREDREQ(string DataContent)
        {
            CCREDREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<CCREDREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static CCREDPLY _CCREDPLY(string DataContent)
        {
            CCREDPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<CCREDPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
      
        public static APVRYCMP _APVRYCMP(string DataContent)
        {
            APVRYCMP obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<APVRYCMP>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static APVRYREQ _APVRYREQ(string DataContent)
        {
            APVRYREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<APVRYREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static APVRYPLY _APVRYPLY(string DataContent)
        {
            APVRYPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<APVRYPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
    }
}

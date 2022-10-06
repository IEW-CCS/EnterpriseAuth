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

        //--------  TryPaser Json File Call 法 --------
        //  if (TryParseJson(DecryptVryopeData, out clsVryope tmpVryopeData)) 
        public static bool TryParseJson<T>(string data, out T result)
        {
            bool success = true;
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Error
            };
            result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data, settings);
            return success;
        }

        /*
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

        public static WSTrx _WSTrx(string DataContent)
        {
            WSTrx obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<WSTrx>(DataContent);
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
        public static ARREGREQ _APREGREQ(string DataContent)
        {
            ARREGREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<ARREGREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static ARREGPLY _APREGPLY(string DataContent)
        {
            ARREGPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<ARREGPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }


        public static ARWSCANN _ARWSCANN(string DataContent)
        {
            ARWSCANN obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<ARWSCANN>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }



        public static ARREGFIN _AREGFIN(string DataContent)
        {
            ARREGFIN obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<ARREGFIN>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }


        public static AACONREQ _AACONREQ(string DataContent)
        {
            AACONREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AACONREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }

        public static AACONPLY _AACONPLY(string DataContent)
        {
            AACONPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AACONPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }

        public static AAPSWREQ _AAPSWREQ(string DataContent)
        {
            AAPSWREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AAPSWREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }

        public static AAPSWPLY _AAPSWPLY(string DataContent)
        {
            AAPSWPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AAPSWPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }


        public static ARREGCMP _APREGCMP(string DataContent)
        {
            ARREGCMP obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<ARREGCMP>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static CRCRLREQ _CCREDREQ(string DataContent)
        {
            CRCRLREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<CRCRLREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static CRCRLPLY _CCREDPLY(string DataContent)
        {
            CRCRLPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<CRCRLPLY>(DataContent);
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
        public static AAUTHREQ _APVRYREQ(string DataContent)
        {
            AAUTHREQ obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AAUTHREQ>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        public static AAUTHPLY _APVRYPLY(string DataContent)
        {
            AAUTHPLY obj = null;
            try
            {
                obj = JsonConvert.DeserializeObject<AAUTHPLY>(DataContent);
            }
            catch
            {
                obj = null;
            }
            return obj;
        }
        */
    }
}

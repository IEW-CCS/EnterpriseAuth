using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth
{
    public static class GlobalVaraible
    {
        public const string DATETIME_FORMAT = "yyyyMMddHHmmssfff";

        public const string WELCOME_VIEWMODEL_NAME = "Welcome";
        public const string REGISTER_VIEWMODEL_NAME = "Register";
        public const string CONNECT_VIEWMODEL_NAME = "Connect";

        public const string PROFILE_STATE_NEW = "New";
        public const string PROFILE_STATE_REGISTER = "Register";

        public const string REGISTER_LOGIN = "api/login/regLogin";

        public const string CONNECTION_TYPE_CITRIX = "Citrixx";
        public const string CONNECTION_TYPE_VPN = "VPN";

        public const int MEDIA_INDEX_CONNECT = 0;
        public const int MEDIA_INDEX_QRCODE = 1;
        public const int MEDIA_INDEX_BLUETOOTH = 2;
    }
}

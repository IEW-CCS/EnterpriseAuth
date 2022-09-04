using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EnterpriseAuth
{
    public class ProfileEditEventArgs : EventArgs
    {
        public string profileName { get; set; }
        public string serverIP { get; set; }
        public string portID { get; set; }
        public string userName { get; set; }
        public string connectionType { get; set; }
        public string connectionImage { get; set; }

    }

    public class BLEMessageEventArgs : EventArgs
    {
        public string bleMessage { get; set; }
    }

    [Serializable]
    public class ServerProfile
    {
        public string strProfileName { get; set; }
        public string strProfileImage { get; set; }
        public string strProfileState { get; set; }
        public string strServerURL { get; set; }
        public string strServerPort { get; set; }
        public string strServerConnectionType { get; set; }
        public string strUserName { get; set; }
        public string strPassword { get; set; }
        public string strMyPrivateKey { get; set; }
        public string strPublicKey { get; set; }
        public string strCredential { get; set; }
        public ServerProfile()
        {
            this.strProfileName = "";
            this.strProfileImage = "";
            this.strProfileState = "";
            this.strServerURL = "";
            this.strServerPort = "";
            this.strServerConnectionType = "";
            this.strUserName = "";
            this.strPassword = "";
            this.strMyPrivateKey = "";
            this.strPublicKey = "";
            this.strCredential = "";
        }
    }
}
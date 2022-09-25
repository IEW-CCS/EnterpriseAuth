using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using EnterpriseAuth.Transactions;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WebSocket4Net;
using EnterpriseAuth.Managers;
using EnterpriseAuth.Security;

namespace EnterpriseAuth.ViewModels
{
    public class ConnectViewModel : BaseViewModel
    {
        //public event EventHandler ProfileUpdateEventHandler;

        public ServerProfile serverProfile = new ServerProfile();
        private string messageInfo = "";
        private BitmapImage stateImage = new BitmapImage();
        private string userPassword = "";
        BlueToothManager btManager = new BlueToothManager();

        public string ProfileName
        {
            get { return serverProfile.strProfileName; }
            set { serverProfile.strProfileName = value; }
        }
        public string ServerIP
        {
            get { return serverProfile.strServerURL; }
            set { serverProfile.strServerURL = value; }
        }
        public string ServerPort
        {
            get { return serverProfile.strServerPort; }
            set { serverProfile.strServerPort = value; }
        }
        public string UserName
        {
            get { return serverProfile.strUserName; }
            set { serverProfile.strUserName = value; }
        }

        public string UserPassword
        {
            //get { return serverProfile.strPassword; }
            set { this.userPassword = value; }
        }
        public string MessageInfo
        {
            get { return this.messageInfo; }
            set
            {
                this.messageInfo = value;
                OnPropertyChanged("MessageInfo");
            }
        }

        public BitmapImage StateImage
        {
            get { return this.stateImage; }
            set
            {
                this.stateImage = value;
                OnPropertyChanged("StateImage");
            }
        }

        public ConnectViewModel(ServerProfile sProfile)
        {
            this.serverProfile = sProfile;
        }

        public void TestAuthentication()
        {

        }
    }
}

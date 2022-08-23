using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EnterpriseAuth;

namespace EnterpriseAuth.ViewModels
{
    public class ConnectViewModel : BaseViewModel
    {
        public ServerProfile serverProfile = new ServerProfile();

        public ServerProfile ProfileObject
        {
            get { return serverProfile; }
            set { serverProfile = value; }
        }

        public string ServerName
        {
            get { return serverProfile.strProfileName; }
            set { serverProfile.strProfileName = value; }
        }

        public ConnectViewModel(ServerProfile sProfile)
        {
            this.serverProfile = sProfile;
        }
    }
}

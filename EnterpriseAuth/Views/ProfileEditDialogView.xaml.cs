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
using System.Windows.Shapes;

namespace EnterpriseAuth.Views
{
    public partial class ProfileEditDialogView : Window
    {
        public event EventHandler ProfileEditEventHandler;

        public ProfileEditDialogView()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ProfileEditEventArgs args = new ProfileEditEventArgs();

            args.profileName = txtProfileName.Text;
            args.serverIP = txtServerIP.Text;
            args.portID = txtServerPort.Text;
            args.userName = txtUserName.Text;

            if(radioCitrix.IsChecked == true)
            {
                args.connectionType = GlobalVaraible.CONNECTION_TYPE_CITRIX;
                args.connectionImage = "images\\citrix_icon.png";
            } else if(radioVPN.IsChecked == true)
            {
                args.connectionType = GlobalVaraible.CONNECTION_TYPE_VPN;
                args.connectionImage = "images\\vpn_icon.png";
            }

            ProfileEditEventHandler(this, args);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

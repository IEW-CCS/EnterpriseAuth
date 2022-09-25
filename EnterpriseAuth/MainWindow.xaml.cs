using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EnterpriseAuth.ViewModels;
using EnterpriseAuth.Views;
using EnterpriseAuth.Managers;
using EnterpriseAuth.Security;

namespace EnterpriseAuth
{
    public partial class MainWindow : Window
    {
        public int iSelectedIndex = -1;
        public List<ServerProfile> profileList = new List<ServerProfile>();
        public BlueToothManager _btm;
        public  static SecurityManager ObjectSecutiry = new SecurityManager();
        public MainWindow()
        {
            InitializeComponent();

            this.ReadProfiles();
            this.listServerProfile.ItemsSource = this.profileList;

            MainWindowViewModel mainViewModel = DataContext as MainWindowViewModel;
            if(mainViewModel.SelectedViewModel == null)
            {
                Console.WriteLine("mainViewModel.SelectedViewModel is Null");
            }
            else
            {
                Console.WriteLine("mainViewModel.SelectedViewModel is Non-Null");
                mainViewModel.SelectedViewModel.ProfileUpdateEventHandler += ProfileUpdateEvent;
            }
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileEditDialogView editDialog = new ProfileEditDialogView();
            editDialog.ProfileEditEventHandler += ProfileAddEventHandler;
            editDialog.ShowDialog();
        }

        public void ProfileAddEventHandler(object sender, EventArgs e)
        {
            ProfileEditEventArgs pe = e as ProfileEditEventArgs;
            AuthSecurity AuthSecurity = new AuthSecurity();

            this.profileList.Add(new ServerProfile()
            {
                strProfileName = pe.profileName,
                strProfileImage = pe.connectionImage,
                strProfileState = GlobalVaraible.PROFILE_STATE_NEW,
                strServerURL = pe.serverIP,
                strServerPort = pe.portID,
                strServerConnectionType = pe.connectionType,
                strUserName = pe.userName,
                strMyPrivateKey = AuthSecurity.PrivateKey,
                strMyPublicKey = AuthSecurity.PublicKey
            });

            ObjectSecutiry.SetRSASecurity(pe.userName, pe.profileName, AuthSecurity);
            this.listServerProfile.Items.Refresh();
        }

        public void ProfileUpdateEvent(object sender, EventArgs e)
        {
            Console.WriteLine("MainWindow.xaml receives ProfileUpdateEventHandler");
            SaveProfiles();
            this.listServerProfile.Items.Refresh();

            
            if (this.iSelectedIndex >= 0)
            {
                //SaveProfiles();
                this.listServerProfile.SelectedIndex = this.iSelectedIndex;
                MainWindowViewModel mainViewModel = DataContext as MainWindowViewModel;
                mainViewModel.UpdateViewCommand.Execute(this.profileList[this.listServerProfile.SelectedIndex]);
            }
            
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Delete Server Profile", "Message");
            var result = MessageBox.Show("Do you want to delete this Server Profile?", "Confirm Message", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning, MessageBoxResult.No);
            if(result == MessageBoxResult.Yes)
            {
                this.profileList.RemoveAt(this.listServerProfile.SelectedIndex);
            }
            MainWindowViewModel mainViewModel = DataContext as MainWindowViewModel;
            mainViewModel.UpdateViewCommand.Execute(new ServerProfile());
            this.listServerProfile.Items.Refresh();
            SaveProfiles();
        }


        private void ServerProfileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.iSelectedIndex = this.listServerProfile.SelectedIndex;

            MainWindowViewModel mainViewModel = DataContext as MainWindowViewModel;

            if(this.listServerProfile.SelectedIndex < 0)
            {
                mainViewModel.UpdateViewCommand.Execute(new ServerProfile());
                return;
            }

            if (this.profileList[this.listServerProfile.SelectedIndex].strProfileState == GlobalVaraible.PROFILE_STATE_NEW)
            {
                mainViewModel.UpdateViewCommand.Execute(this.profileList[this.listServerProfile.SelectedIndex]);
                mainViewModel.SelectedViewModel.ProfileUpdateEventHandler += ProfileUpdateEvent;
            }
            else if (this.profileList[this.listServerProfile.SelectedIndex].strProfileState == GlobalVaraible.PROFILE_STATE_REGISTER)
            {
                mainViewModel.UpdateViewCommand.Execute(this.profileList[this.listServerProfile.SelectedIndex]);
                mainViewModel.SelectedViewModel.ProfileUpdateEventHandler += ProfileUpdateEvent;
            }
            else
            {
                mainViewModel.UpdateViewCommand.Execute(new ServerProfile());
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(this._btm != null)
            {
                this._btm.DisconnectDevice();
                this._btm = null;
                GC.Collect();
            }

            SaveProfiles();
        }

        private void ReadProfiles()
        {
            ///Need to Add Error Handling

            IFormatter formatter = new BinaryFormatter();

            Stream stream = new FileStream(@".\profile.dat", FileMode.Open, FileAccess.Read);
            this.profileList = (List<ServerProfile>)formatter.Deserialize(stream);

            foreach( var profile in profileList)
            {
                string username = profile.strUserName;
                string server = profile.strProfileName;
                string publicKey = profile.strMyPublicKey;
                string privateKey = profile.strMyPrivateKey;
                string ServerPublic = profile.strServerPublicKey;

                if (username != string.Empty  && server != string.Empty && publicKey != string.Empty && privateKey != string.Empty)
                {
                    AuthSecurity AuthS = new AuthSecurity(privateKey, publicKey);
                    AuthS.ClientID = server;
                    AuthS.ClientPublicKey = ServerPublic;
                    ObjectSecutiry.SetRSASecurity(username, server, AuthS);
                }
            }
            
        }

        private void SaveProfiles()
        {

            foreach (var profile in profileList)
            {
                string username = profile.strUserName;
                string server = profile.strProfileName;

                AuthSecurity AuthS  = ObjectSecutiry.GetRSASecurity(profile.strUserName, profile.strProfileName);
                profile.strMyPublicKey = AuthS.PublicKey;
                profile.strMyPrivateKey = AuthS.PrivateKey;
                profile.strServerPublicKey = AuthS.ClientPublicKey;

            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@".\profile.dat", FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, this.profileList);
            stream.Close();
        }

        /*
        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Start to scan devices......");
            BlueToothManager btm = new BlueToothManager();
            this._btm = btm;
            btm.ScanAndConnect();
        }
        */
    }
}
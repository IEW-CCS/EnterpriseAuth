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

namespace EnterpriseAuth
{
    public partial class MainWindow : Window
    {

        public List<ServerProfile> profileList = new List<ServerProfile>();
        public BlueToothManager _btm;
        public MainWindow()
        {
            InitializeComponent();

            this.ReadProfiles();
            this.listServerProfile.ItemsSource = this.profileList;
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

            this.profileList.Add(new ServerProfile()
            {
                strProfileName = pe.profileName,
                strProfileImage = pe.connectionImage,
                strProfileState = GlobalVaraible.PROFILE_STATE_NEW,
                strServerURL = pe.serverIP,
                strServerPort = pe.portID,
                strServerConnectionType = pe.connectionType,
                strUserName = pe.userName,
            });

            this.listServerProfile.Items.Refresh();
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
            MainWindowViewModel mainViewModel = DataContext as MainWindowViewModel;

            if(this.listServerProfile.SelectedIndex < 0)
            {
                mainViewModel.UpdateViewCommand.Execute(new ServerProfile());
                return;
            }

            if (this.profileList[this.listServerProfile.SelectedIndex].strProfileState == GlobalVaraible.PROFILE_STATE_NEW)
            {
                mainViewModel.UpdateViewCommand.Execute(this.profileList[this.listServerProfile.SelectedIndex]);
            }
            else if (this.profileList[this.listServerProfile.SelectedIndex].strProfileState == GlobalVaraible.PROFILE_STATE_REGISTER)
            {
                mainViewModel.UpdateViewCommand.Execute(this.profileList[this.listServerProfile.SelectedIndex]);
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

        }

        private void SaveProfiles()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@".\profile.dat", FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, this.profileList);
            stream.Close();
        }

        public async void TestCitrix()
        {
            //string strUrl = "https://roaming.innolux.com/nitro/v1/config/login";
            string strUrl = "https://roaming.innolux.com/logon/LogonPoint";
            string strJson = @"{""login"": ""username"": ""james.loa"", ""password"": ""!3edc4rfv""}}";

            StringContent requestContent = new StringContent(strJson, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(strUrl);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(strUrl, requestContent);
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        string jsonContent = await content.ReadAsStringAsync();
                        Console.Write(jsonContent);
                    }
                    else
                    {
                        //Application.Current.Dispatcher.Invoke(() => {
                        //    this.MessageInfo = "HTTP Response Error: " + response.StatusCode;
                        //}, DispatcherPriority.Background);
                        Console.WriteLine("HTTP Response error");
                    }
                }
                catch (Exception ex)
                {
                    //Application.Current.Dispatcher.Invoke(() => {
                    //    this.MessageInfo = "HTTP Post Exception: " + ex.Message;
                    //}, DispatcherPriority.Background);
                    Console.WriteLine("HTTP Exception: " + ex.Message);
                }
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Start to scan devices......");
            BlueToothManager btm = new BlueToothManager();
            this._btm = btm;
            btm.TestScan();
        }
    }
}
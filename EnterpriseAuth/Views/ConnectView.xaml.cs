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
using System.Drawing;
using System.Threading;
using EnterpriseAuth.Transactions;
using EnterpriseAuth.ViewModels;


namespace EnterpriseAuth.Views
{
    /// <summary>
    /// ConnectView.xaml 的互動邏輯
    /// </summary>
    public partial class ConnectView : UserControl
    {
        public ConnectViewModel connectViewModel;

        public ConnectView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.connectViewModel = DataContext as ConnectViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.registerViewModel.RegisterServer();
            string passwd = this.pwdBox.Password;
            this.connectViewModel.UserPassword = passwd;
            this.connectViewModel.StartAuthentication();
        }

    }
}

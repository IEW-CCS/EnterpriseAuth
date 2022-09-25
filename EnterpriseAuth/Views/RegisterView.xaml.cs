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
    public partial class RegisterView : UserControl
    {
        public int index = 0;
        public RegisterViewModel registerViewModel;
        public RegisterView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.registerViewModel = DataContext as RegisterViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.registerViewModel.RegisterServer();
            this.registerViewModel.TestRegisterComplete();
        }

    }
}

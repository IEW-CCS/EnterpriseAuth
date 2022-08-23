using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EnterpriseAuth.ViewModels;

namespace EnterpriseAuth.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private MainWindowViewModel viewModel;

        public UpdateViewCommand(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //MessageBox.Show("Parameter object = " + parameter.ToString(), "Message");
            if (parameter == null)
            {
                return;
            }

            ServerProfile sProfile = new ServerProfile();
            sProfile = parameter as ServerProfile;

            if (sProfile.strProfileState == GlobalVaraible.PROFILE_STATE_NEW)
            {
                viewModel.SelectedViewModel = new RegisterViewModel(sProfile);
            }
            else if (sProfile.strProfileState == GlobalVaraible.PROFILE_STATE_REGISTER)
            {
                viewModel.SelectedViewModel = new ConnectViewModel(sProfile);
            }
            else
            {
                viewModel.SelectedViewModel = new WelcomeViewModel();
            }

        }
    }
}

using EnterpriseAuth.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EnterpriseAuth.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        //public event EventHandler ProfileUpdateEventHandler;

        private BaseViewModel _selectedViewModel;
        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                _selectedViewModel = value;
                OnPropertyChanged(nameof(SelectedViewModel));
            }
        }

        public ICommand UpdateViewCommand { get; set; }

        public MainWindowViewModel()
        {
            this.UpdateViewCommand = new UpdateViewCommand(this);
            this.UpdateViewCommand.Execute(new ServerProfile());
        }

    }

}

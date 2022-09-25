using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAuth.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        //public Action<object, EventArgs> ProfileUpdateEventHandler { get; internal set; }

        public event EventHandler ProfileUpdateEventHandler;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnProfileUpdated()
        {
            var handler = ProfileUpdateEventHandler;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}

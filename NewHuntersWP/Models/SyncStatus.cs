using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HuntersWP.Models
{
    class SyncStatus:INotifyPropertyChanged
    {
        private string _syncStatusText;

        public string SyncStatusText
        {
            get { return _syncStatusText; }
            set
            {
                _syncStatusText = value;
                NotifyPropertyChanged("SyncStatusText");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) { if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); } }


    }
}

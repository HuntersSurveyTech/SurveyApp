using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using HuntersWP.Db;
using HuntersWP.Pages;
using HuntersWP.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace HuntersWP.Models
{
    public class BasePage : PhoneApplicationPage
    {
        public BasePage()
        {
            this.Loaded += BasePage_Loaded;
            this.Unloaded += BasePage_Unloaded;
        }

        void BasePage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        void BasePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SyncEngine.SyncCompleted -= SyncEngine_SyncCompleted;
            SyncEngine.SyncStarted -= SyncEngine_SyncStarted;
            SyncEngine.SyncCompleted += SyncEngine_SyncCompleted;
            SyncEngine.SyncStarted += SyncEngine_SyncStarted;
            this.Loaded -= BasePage_Loaded;
            CheckIndicator();
            RefreshSyncStatus();

        }

        void SyncEngine_SyncStarted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                CheckIndicator();
                RefreshSyncStatus();
            });
        }

        void SyncEngine_SyncCompleted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                CheckIndicator();
                RefreshSyncStatus();
            });
        }

        //protected void RefreshSyncStatus()
        //{
        //    if (SystemTray.ProgressIndicator == null) return;
        //    SystemTray.ProgressIndicator.SetValue(ProgressIndicator.IsVisibleProperty, SyncEngine.IsSyncing);
        //    if (SyncEngine.IsSyncing)
        //    {
        //        SystemTray.ProgressIndicator.SetValue(ProgressIndicator.TextProperty, "Syncing");
        //    }
        //    else
        //    {
        //        SystemTray.ProgressIndicator.SetValue(ProgressIndicator.TextProperty, "");
        //    }
        //}

        protected void RefreshSyncStatus(TextBlock synTextBlock = null)
        {
            if (SystemTray.ProgressIndicator == null) return;
            SystemTray.ProgressIndicator.SetValue(ProgressIndicator.IsVisibleProperty, SyncEngine.IsSyncing);
            if (SyncEngine.IsSyncing)
            {
                SystemTray.ProgressIndicator.SetValue(ProgressIndicator.TextProperty, "Syncing");
            }
            else
            {
                SystemTray.ProgressIndicator.SetValue(ProgressIndicator.TextProperty, "");
                if (synTextBlock != null)
                {
                    UpdateSyncStatusText(synTextBlock);
                }
            }
        }

        private async void UpdateSyncStatusText(TextBlock synTextBlock)
        {
            var addresses = await new DbService().GetNotSyncedEntities<Address>();
            var survelems = await new DbService().GetNotSyncedEntities<Survelem>();
            var medias = await new DbService().GetNotSyncedEntities<RichMedia>();

            if (addresses.Count > 0 || survelems.Count > 0 || medias.Count > 0)
            {
                synTextBlock.Text = "NOT Synced";
            }
            else
            {
                synTextBlock.Text = "Synced";
            }
        }

        public string SyncStatusText { get; set; }


        void CheckIndicator()
        {
            ProgressIndicator progressIndicator = SystemTray.ProgressIndicator;
            if (progressIndicator == null)
            {
                progressIndicator = new ProgressIndicator();
                progressIndicator.SetValue(ProgressIndicator.IsIndeterminateProperty, true);
                SystemTray.SetProgressIndicator(this, progressIndicator);
            } 
        }

        protected bool isBusy;
        public bool IsBusy
        {
            set
            {
                isBusy = value;

               // CheckIndicator();
                
              //  SystemTray.ProgressIndicator.SetValue(ProgressIndicator.IsVisibleProperty, value);
        
            }
            get
            {
                return isBusy;
            }
        }
    }
}

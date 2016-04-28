using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Storage;
using HuntersWP.Db;
using HuntersWP.Models;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HuntersWP.Services
{
    public static class SyncEngine
    {
        static readonly object _locker = new object();
        private static bool _isSyncing;

        private static DispatcherTimer _timer;

        public static void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 15, 0);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        static void _timer_Tick(object sender, EventArgs e)
        {
            if (Helpers.IsNetworkAvailable)
            {
                Sync();
            }
        }

        public static event EventHandler SyncCompleted = delegate { };
        public static event EventHandler SyncStarted = delegate { };
        public static bool IsSyncing
        {
            get
            {
                lock (_locker)
                {
                    return _isSyncing;
                }
            }
        }

        static async Task Execute(Func<Task> syncAction)
        {
            lock (_locker)
            {
                if (_isSyncing) return;
                _isSyncing = true;
            }

            try
            {
                await syncAction();
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR: " + e.Message);
                //MessageBox.Show("SYNC Error: " + e.Message);
            }
            finally
            {
                _isSyncing = false;

                SyncCompleted(null, EventArgs.Empty);
            }
        }

        //public static async Task Sync()
        //{
        //    if (!Helpers.IsNetworkAvailable) return;

        //    await Execute(InternalSync);
        //}

        public static async Task Sync(bool force = false)
        {
            if (!force && !Helpers.IsNetworkAvailable) return;

            await Execute(InternalSync);

        }

        private static int _counter;

        private static readonly object _lockerForSync = new object();

 //       static async void ProcessItem(Survelem survelem, NetmeraContent e, TaskCompletionSource<object> res)
 //       {
 //           await new DbService().Save(survelem, ESyncStatus.InProcess, null, true);

 //           var r = survelem.IsCreatedOnClient
 //? await new MyNetmeraClient().Create<Survelem>(e, survelem.id)
 //: await new MyNetmeraClient().Update<Survelem>(e, survelem.id);
            
 //           Debug.WriteLine("Processing Survelem");


 //           survelem.IsCreatedOnClient = false;


 //           if (r.IsSuccess)
 //           {
 //               await new DbService().Save(survelem, ESyncStatus.Success,null,true);
 //           }
 //           else
 //           {
 //               await new DbService().Save(survelem, ESyncStatus.Error, r.Exception,true);
 //           }


 //           lock (_lockerForSync)
 //           {
 //               _counter--;

 //               if (_counter == 0)
 //               {
 //                   res.SetResult(null);
 //               }
 //           }
 //       }


//        private static async Task<bool> ProcessSurvelemesWithBatches()
//        {
            
//            Debug.WriteLine("Processing survelems with batches");
//            var survelems = await new DbService().GetNotSyncedEntities<Survelem>();

//            var batch = 30;

//            var items = survelems.Where(x => x.SyncStatus == (byte) ESyncStatus.InProcess).ToList();


//            if (items.Any())
//            {
//                for (int i = 0; i <= items.Count/batch; i++)
//                {
//                    var syncItems = items.Skip(i*batch).Take(batch);

//                    var serviceResult =
//                        await
//                            new MyNetmeraClient().FindNetmeraContents<Survelem>("Survelem", "id",
//                                syncItems.Select(x => x.id).ToList());

//                    if (serviceResult.IsSuccess)
//                    {
//                        foreach (var item in serviceResult.Data)
//                        {
//                            var ii = items.FirstOrDefault(x => x.id == item.id);
//                            if (ii != null)
//                            {
//                                ii.IsCreatedOnClient = false;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        return false;
//                    }

//                }
//            }

//            var toCreate = survelems.Where(x => x.IsCreatedOnClient).ToList();
//            var toUpdate = survelems.Where(x => !x.IsCreatedOnClient).ToList();


//#if DEBUG
//           //CheckDataDuplication(toCreate);

//           //CheckDataDuplication(toUpdate);
//#endif


//            var r = await ProcessSurvelemsList(toCreate, true);
//            if (!r) return r;
//            r = await ProcessSurvelemsList(toUpdate, false);

//            Debug.WriteLine("Processing survelems with batches finished");
//            return r;

//        }


    

      

        static async Task InternalSync()
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            SyncStarted(null, EventArgs.Empty);

            Debug.WriteLine("Sync started");


            await InternalSyncEngine.Execute(StateService.IsQA);

            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            Debug.WriteLine("Sync finished");
        }

       
    }
}

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
using Netmera;

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


        private static async Task<bool> ProcessSurvelemesWithBatches()
        {
            
            Debug.WriteLine("Processing survelems with batches");
            var survelems = await new DbService().GetNotSyncedEntities<Survelem>();

            var batch = 30;

            var items = survelems.Where(x => x.SyncStatus == (byte) ESyncStatus.InProcess).ToList();


            if (items.Any())
            {
                for (int i = 0; i <= items.Count/batch; i++)
                {
                    var syncItems = items.Skip(i*batch).Take(batch);

                    var serviceResult =
                        await
                            new MyNetmeraClient().FindNetmeraContents<Survelem>("Survelem", "id",
                                syncItems.Select(x => x.id).ToList());

                    if (serviceResult.IsSuccess)
                    {
                        foreach (var item in serviceResult.Data)
                        {
                            var ii = items.FirstOrDefault(x => x.id == item.id);
                            if (ii != null)
                            {
                                ii.IsCreatedOnClient = false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                }
            }

            var toCreate = survelems.Where(x => x.IsCreatedOnClient).ToList();
            var toUpdate = survelems.Where(x => !x.IsCreatedOnClient).ToList();


#if DEBUG
           //CheckDataDuplication(toCreate);

           //CheckDataDuplication(toUpdate);
#endif


            var r = await ProcessSurvelemsList(toCreate, true);
            if (!r) return r;
            r = await ProcessSurvelemsList(toUpdate, false);

            Debug.WriteLine("Processing survelems with batches finished");
            return r;

        }


        static void CheckDataDuplication(List<Survelem> survelems)
        {
            var dict = new Dictionary<string, string>();

            List<string> _duplicates = new List<string>();
            foreach (var s in survelems)
            {
                var key = string.Format("{0};{1};{2}", s.CustomerID,s.UPRN,s.Question_Ref);

                if (dict.ContainsKey(key))
                {
                    if(!_duplicates.Contains(key))
                        _duplicates.Add(key);
                }
                else
                {
                    dict.Add(key,s.id);
                }

            }

            if (_duplicates.Any())
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    throw new Exception("data duplication");
                }
            }

        }

        private static async Task<bool> ProcessSurvelemsList(List<Survelem> survelems, bool create)
        {
            Debug.WriteLine("ProcessSurvelemsList:" + survelems.Count);
           var entities = new List<Dictionary<string, string>>();

            if (survelems.Any())
            {
                foreach (var survelem in survelems)
                {
                    var e = new Dictionary<string, string>();
                    e.Add("COMMENT", survelem.COMMENT);
                    e.Add("CustomerID", survelem.CustomerID.ToString());
                    e.Add("CustomerSurveyID", survelem.CustomerSurveyID);
                    e.Add("OptionID", survelem.OptionID ?? "");
                    e.Add("OptionID2ndry", survelem.OptionID2ndry ?? "");
                    e.Add("Question_Ref", survelem.Question_Ref);
                    e.Add("UPRN", survelem.UPRN);
                    e.Add("id", survelem.id);
                    e.Add("Freetext", survelem.Freetext ?? "");
                    e.Add("BuildingType", survelem.BuildingType ?? "");
                    e.Add("DateOfSurvey", survelem.DateOfSurvey);

                    e.Add("SqN1", survelem.SqN1 ?? "");
                    e.Add("SqN10", survelem.SqN10 ?? "");
                    e.Add("SqN11", survelem.SqN11 ?? "");
                    e.Add("SqN12", survelem.SqN12 ?? "");
                    e.Add("SqN13", survelem.SqN13 ?? "");
                    e.Add("SqN14", survelem.SqN14 ?? "");
                    e.Add("SqN15", survelem.SqN15 ?? "");
                    e.Add("SqN2", survelem.SqN2 ?? "");
                    e.Add("SqN3", survelem.SqN3 ?? "");
                    e.Add("SqN4", survelem.SqN4 ?? "");
                    e.Add("SqN5", survelem.SqN5 ?? "");
                    e.Add("SqN6", survelem.SqN6 ?? "");
                    e.Add("SqN7", survelem.SqN7 ?? "");
                    e.Add("SqN8", survelem.SqN8 ?? "");
                    e.Add("SqN9", survelem.SqN9 ?? "");
                    e.Add("SqT1", survelem.SqT1 ?? "");
                    e.Add("SqT10", survelem.SqT10 ?? "");
                    e.Add("SqT11", survelem.SqT11 ?? "");
                    e.Add("SqT12", survelem.SqT12 ?? "");
                    e.Add("SqT13", survelem.SqT13 ?? "");
                    e.Add("SqT14", survelem.SqT14 ?? "");
                    e.Add("SqT15", survelem.SqT15 ?? "");
                    e.Add("SqT2", survelem.SqT2 ?? "");
                    e.Add("SqT3", survelem.SqT3 ?? "");
                    e.Add("SqT4", survelem.SqT4 ?? "");
                    e.Add("SqT5", survelem.SqT5 ?? "");
                    e.Add("SqT6", survelem.SqT6 ?? "");
                    e.Add("SqT7", survelem.SqT7 ?? "");
                    e.Add("SqT8", survelem.SqT8 ?? "");
                    e.Add("SqT9", survelem.SqT9 ?? "");

                    entities.Add(e);

                    Debug.WriteLine("Survelem added:" + survelem.id);
                }

                if (entities.Any())
                {
                    foreach (var entity in survelems)
                    {

                        await new DbService().Save(entity, ESyncStatus.InProcess, null, true);
                    }
                    var r = await new MyNetmeraClient().BatchSave("Survelem", entities, survelems, create);

                    if (r.Exception != null)
                        return false;
                }
            }
            return true;
        }

//        static async Task ProcessSurvelemes()
//        {
//            var requestCount = 10;

            

//            var survelems = await new DbService().GetNotSyncedEntities<Survelem>();

//            for (int i = 0; i <= survelems.Count / requestCount; i++)
//            {
//                TaskCompletionSource<object> res = new TaskCompletionSource<object>();
//;

//                var items = survelems.Skip(i*requestCount).Take(requestCount).ToList();

//                _counter = items.Count();
//                Debug.WriteLine("Processign survelems: " + _counter);

//                if (items.Any())
//                {
//                    foreach (var survelem in items)
//                    {

//                        NetmeraContent e = null;
//                        if (survelem.IsCreatedOnClient)
//                        {
//                            e = new NetmeraContent("Survelem");
//                        }
//                        else
//                        {
//                            e = await new MyNetmeraClient().FindNetmeraContent("Survelem", "id", survelem.id);
//                        }

//                        if (e != null)
//                        {
//                            e.add("COMMENT", survelem.COMMENT);
//                            e.add("CustomerID", survelem.CustomerID);
//                            e.add("CustomerSurveyID", survelem.CustomerSurveyID);
//                            e.add("DateOfSurvey", survelem.DateOfSurvey);
//                            e.add("OptionID", survelem.OptionID ?? "");
//                            e.add("OptionID2ndry", survelem.OptionID2ndry ?? "");
//                            e.add("Question_Ref", survelem.Question_Ref);
//                            e.add("UPRN", survelem.UPRN);
//                            e.add("id", survelem.id);
//                            e.add("Freetext", survelem.Freetext ?? "");
//                            e.add("BuildingType", survelem.BuildingType ?? "");
                            

//                            e.add("SqN1", survelem.SqN1 ?? "");
//                            e.add("SqN10", survelem.SqN10 ?? "");
//                            e.add("SqN11", survelem.SqN11 ?? "");
//                            e.add("SqN12", survelem.SqN12 ?? "");
//                            e.add("SqN13", survelem.SqN13 ?? "");
//                            e.add("SqN14", survelem.SqN14 ?? "");
//                            e.add("SqN15", survelem.SqN15 ?? "");
//                            e.add("SqN2", survelem.SqN2 ?? "");
//                            e.add("SqN3", survelem.SqN3 ?? "");
//                            e.add("SqN4", survelem.SqN4 ?? "");
//                            e.add("SqN5", survelem.SqN5 ?? "");
//                            e.add("SqN6", survelem.SqN6 ?? "");
//                            e.add("SqN7", survelem.SqN7 ?? "");
//                            e.add("SqN8", survelem.SqN8 ?? "");
//                            e.add("SqN9", survelem.SqN9 ?? "");
//                            e.add("SqT1", survelem.SqT1 ?? "");
//                            e.add("SqT10", survelem.SqT10 ?? "");
//                            e.add("SqT11", survelem.SqT11 ?? "");
//                            e.add("SqT12", survelem.SqT12 ?? "");
//                            e.add("SqT13", survelem.SqT13 ?? "");
//                            e.add("SqT14", survelem.SqT14 ?? "");
//                            e.add("SqT15", survelem.SqT15 ?? "");
//                            e.add("SqT2", survelem.SqT2 ?? "");
//                            e.add("SqT3", survelem.SqT3 ?? "");
//                            e.add("SqT4", survelem.SqT4 ?? "");
//                            e.add("SqT5", survelem.SqT5 ?? "");
//                            e.add("SqT6", survelem.SqT6 ?? "");
//                            e.add("SqT7", survelem.SqT7 ?? "");
//                            e.add("SqT8", survelem.SqT8 ?? "");
//                            e.add("SqT9", survelem.SqT9 ?? "");

//                        }

//                        var s = survelem;


//                        ProcessItem(s, e, res);

//                    }
//                }
//                else
//                {
//                    res.SetResult(null);
//                }
            
//                await res.Task;

//            }
//        }

        static async Task InternalSync()
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            SyncStarted(null, EventArgs.Empty);
            Debug.WriteLine("Sync started");
            var addresses = await new DbService().GetNotSyncedEntities<Address>();


            foreach (var item in addresses.Where(x => x.SyncStatus == (byte)ESyncStatus.InProcess))
            {
                var content = await new MyNetmeraClient().FindNetmeraContent("Address_List1", "AddressID", item.AddressID);

                if (content != null)
                {
                    item.IsCreatedOnClient = false;
                }
            }

            foreach (var a in addresses)
            {

                NetmeraContent e = null;
                if (a.IsCreatedOnClient)
                {
                    e = new NetmeraContent("Address_List1");

                }
                else
                {
                    e = await new MyNetmeraClient().FindNetmeraContent("Address_List1", "AddressID", a.AddressID);
                }

                if (e != null)
                {
                    e.add("Address line 1", a.AddressLine1);
                    e.add("Address line 2", a.AddressLine2 ?? "");
                    e.add("Address line 3", a.AddressLine3 ?? "");
                    e.add("Address line 4", a.AddressLine4 ?? "");
                    e.add("Type", a.Type);
                    e.add("PTUpdated", a.PTUpdated);
                    e.add("UPRN", a.UPRN);
                    e.add("FlatNo", a.FlatNo);
                    e.add("Postcode", a.Postcode);
                    e.add("StreetName", a.StreetName);
                    e.add("StreetNo", a.StreetNo);
                    e.add("BlockUPRN", a.BlockUPRN);
                    e.add("BuildingName", a.BuildingName);
                    e.add("CustomerSurveyID", a.CustomerSurveyID);
                    e.add("CustomerID", a.CustomerID);
                    e.add("Surveyor", a.Surveyor ?? "");
                    e.add("AddressID", a.AddressID);
                    e.add("AllowCopyFrom", a.AllowCopyFrom);
                    e.add("Bedrooms", a.Bedrooms ?? "");
                    e.add("Complete", a.Complete);
                    e.add("CopiedFrom", a.CopiedFrom ?? "");
                    e.add("CopyTo", a.CopyTo);
                    e.add("DateSurveyed", a.DateSurveyed ?? "");
                    e.add("Floor", a.Floor ?? "");
                    e.add("FullAddress", a.FullAddress ?? "");
                    e.add("LeaseHolderAddress", a.LeaseHolderAddress ?? "");
                    e.add("Multipliers", a.Multipliers ?? "");
                    e.add("QuestionGrp", a.QuestionGrp ?? "");
                    e.add("SAPBand", a.SAPBand ?? "");
                    e.add("SAPRating", a.SAPRating ?? "");
                    e.add("Submit", a.Submit ?? "");
                    e.add("Submitted", a.Submitted ?? "");
                    e.add("Visited", a.Visited ?? "");
                }

                await new DbService().Save(a, ESyncStatus.InProcess, null, true);

                var r = a.IsCreatedOnClient ? await new MyNetmeraClient().Create<Address>(e, a.AddressID) : await new MyNetmeraClient().Update<Address>(e, a.AddressID);

                a.IsCreatedOnClient = false;
                if (r.IsSuccess)
                {
                    await new DbService().Save(a, ESyncStatus.Success,null,true);
                }
                else
                {
                    await new DbService().Save(a, ESyncStatus.Error, r.Exception, true);
                }

            }


            await ProcessSurvelemesWithBatches();

            var medias = await new DbService().GetNotSyncedEntities<RichMedia>();



            foreach (var item in medias.Where(x => x.SyncStatus == (byte)ESyncStatus.InProcess))
            {
                var content = await new MyNetmeraClient().FindNetmeraContent("RichMedia", "ID", item.ID);

                if (content != null)
                {
                    item.IsCreatedOnClient = false;
                }
            }

            foreach (var media in medias)
            {

                NetmeraContent e = null;
                if (media.IsCreatedOnClient)
                {
                    e = new NetmeraContent("RichMedia");
                }
                else
                {
                    e = await new MyNetmeraClient().FindNetmeraContent("RichMedia", "ID", media.ID);
                }

                if (e != null)
                {
                    e.add("Comments", media.Comments);
                    e.add("CustomerID", media.CustomerID);
                    e.add("CustomerSurveyID", media.CustomerSurveyID);
                    e.add("FileName", media.FileName);
                    e.add("ID", media.ID);
                    e.add("Option_ID", media.Option_ID ?? "");
                    e.add("Question_Ref", media.Question_Ref);
                    e.add("UPRN", media.UPRN);


                    using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var file = iso.OpenFile(media.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            var data = new byte[file.Length];

                            await file.ReadAsync(data, 0, data.Length);

                            var path = await UploadToCloud(data);
                            e.add("Photo", path);
                        }
                    }

          
                }


                await new DbService().Save(media, ESyncStatus.InProcess, null, true);

                var r = media.IsCreatedOnClient ? await new MyNetmeraClient().Create<RichMedia>(e,media.ID) : await new MyNetmeraClient().Update<RichMedia>(e,media.ID);


                media.IsCreatedOnClient = false;

                if (r.IsSuccess)
                {
                    await new DbService().Save(media, ESyncStatus.Success,null,true);
                }
                else
                {
                    await new DbService().Save(media, ESyncStatus.Error, r.Exception,true);
                }
            }

            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            Debug.WriteLine("Sync finished");
        }

        private static async Task<string> UploadToCloud(byte[] data)
        {
            var account =
                new CloudStorageAccount(
                    new StorageCredentials("hunters",
                        "hSY6n6KoHvqTfk6NByrZODMlsqWfrA9g030I+Pr9QAzt5NK2vmh0KTzRiydq1kCkFu6Ot3lKAty+30EwjJA2+A=="),
                    true);

            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference("hunters-photos");
            await container.CreateIfNotExistsAsync();

            await container.SetPermissionsAsync(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off });

            var blob = container.GetBlockBlobReference(Guid.NewGuid().ToString());

            await blob.UploadFromByteArrayAsync(data, 0, data.Length);

            return blob.Uri.ToString();
        }
    }
}

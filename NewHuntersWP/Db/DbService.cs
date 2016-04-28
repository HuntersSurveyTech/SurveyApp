using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if !IMPORT
using Windows.Foundation.Metadata;
using Windows.Storage;
#endif
using HuntersWP.Models;
using HuntersWP.Services;
using SQLite;


namespace HuntersWP.Db
{
    public class DbService
    {
        public static string DB_NAME = "db2.sqlite";

        public static string DB_NAME_COPY = "db2copy.sqlite";

#if IMPORT
        public static string DB_PATH = "";
#else
            public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_NAME));
#endif



        #if IMPORT
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(Path.Combine(DB_PATH, DB_NAME));
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return new SQLiteAsyncConnection(Path.Combine(DB_PATH, DB_NAME));
        }
#else
        public SQLiteConnection GetConnection()
        {
            string dbRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return new SQLiteConnection(Path.Combine(dbRootPath, DB_PATH));
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            string dbRootPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return new SQLiteAsyncConnection(Path.Combine(dbRootPath, DB_PATH));
        }
#endif

        public async Task<List<Address>> GetAddressesForCopyTo(string type, string uprn)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<Address>()
                        .Where(x => x.IsCompleted == false && x.CopyTo == true && x.Type == type && x.UPRN != uprn) //bitince x.copyTo=true'ya cevir
                        .ToListAsync();
        }

        public async Task<QAAddress> FindQaAddress(Guid addressId)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<QAAddress>().Where(x => x.AddressId == addressId).FirstAsync();
        }

        public async Task<QAAddressComment> FindQaAddressComment(Guid addressId, string questionRef)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<QAAddressComment>().Where(x => x.AddressId == addressId && x.QuestionRef == questionRef).FirstOrDefaultAsync();
        }

        public async Task<List<Survelem>> GetSurvelemsByAddressUPRN(string uprn)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<Survelem>()
                        .Where(x => x.UPRN == uprn)
                        .ToListAsync();
        }

        public async Task<List<Survelem>> GetSyncedSurvelemsByAddressUPRN(string uprn)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<Survelem>()
                        .Where(x => x.UPRN == uprn && x.SyncStatus == (byte)ESyncStatus.Success)
                        .ToListAsync();
        }


        public async Task<List<RichMedia>> GetRichMediasAddressUPRN(string uprn)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<RichMedia>()
                        .Where(x => x.UPRN == uprn)
                        .ToListAsync();
        }

        public async Task<List<T>> GetNotSyncedEntities<T>() where T : Entity, new()
        {
            return await GetAsyncConnection().Table<T>().Where(x => x.SyncStatus == (byte)ESyncStatus.NotSynced).ToListAsync();
        }

        public async Task<List<T>> GetNotSyncedEntities<T>(int skip, int take) where T : Entity, new()
        {
            return await GetAsyncConnection().Table<T>().Where(x => x.SyncStatus == (byte)ESyncStatus.NotSynced).OrderBy(x=>x.Id).Skip(skip).Take(take).ToListAsync();
        }


        public async Task<int> GetNotSyncedEntitiesCount<T>() where T : Entity, new()
        {
            return
                await
                    GetAsyncConnection()
                        .Table<T>()
                        .Where(x => x.SyncStatus == (byte)ESyncStatus.NotSynced)
                        .CountAsync();
        }

        //public async Task<List<SurvelemSecond>> GetSurvelemSeconds(Guid id)
        //{
        //    return await GetAsyncConnection().Table<SurvelemSecond>().Where(x => x.SurvelemId == id).ToListAsync();
        //}

        public async Task<List<Address>> GetAddressesToRemove()
        {
            return await GetAsyncConnection().Table<Address>().Where(x => x.RemoveDataAfterSync && x.SyncStatus == (byte)ESyncStatus.Success).ToListAsync();
        }

        public async Task<List<Address>> GetAddressesToRemoveIsNotLoadToPhone()
        {
            return await GetAsyncConnection().Table<Address>().Where(x => x.IsCompleted && x.IsLoadToPhone == false && x.SyncStatus == (byte)ESyncStatus.Success).ToListAsync();
        }

        public async Task<List<Survelem>> GetSurvelemsToRemove()
        {
            return await GetAsyncConnection().Table<Survelem>().Where(x => x.RemoveDataAfterSync && x.SyncStatus == (byte)ESyncStatus.Success).ToListAsync();
        }

        public async Task ClearTable<T>(SQLiteAsyncConnection c = null) where T : new()
        {
            var connection = c ?? GetAsyncConnection();

            await connection.DropTableAsync<T>();
            await connection.CreateTableAsync<T>();
        }

        public async Task<List<Question>> GetQuestions(string surveyTypesName, string surveyId)
        {
            //var questions = await GetAsyncConnection().Table<Question>().ToListAsync();
            var questions = await GetAsyncConnection().Table<Question>().Where(x => x.CustomerSurveyID == surveyId).ToListAsync();

            var surveyTypes =
                surveyTypesName.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    //.Select(x => Regex.Replace(x, "[0-9]", ""))
                    .ToList();

            var r = new List<Question>();

            foreach (var question in questions)
            {
                var types = question.SurveyType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    //.Select(x => Regex.Replace(x, "[0-9]", ""))
                    .ToList();

                if (types.Intersect(surveyTypes).Any())
                {
                    r.Add(question);
                }

            }


            return r.OrderBy(x => x.Question_Order).ToList();


        }



        public async Task<int> Count<T>() where T : new()
        {
            return await GetAsyncConnection().Table<T>().CountAsync();
        }


        public async Task<SurveyType> FindSurveyType(string name, string surveyId)
        {
            return await GetAsyncConnection().Table<SurveyType>().Where(x => x.Name == name && x.CustomerSurveyID == surveyId).FirstOrDefaultAsync();
        }


        public async Task<T> Find<T>(object id) where T : Entity, new()
        {
            return await GetAsyncConnection().FindAsync<T>(id);
        }

        public async Task Insert<T>(List<T> items) where T : Entity, new()
        {
            if (items.Count == 0) return;
            var c = GetAsyncConnection();

            foreach (var item in items)
            {
                item.SyncStatus = (byte)ESyncStatus.Success;

            }

            
            await c.InsertAllAsync(items);
        }

        public async Task Save<T>(List<T> items) where T : Entity, new()
        {
            foreach (var item in items)
            {
                await Save<T>(item);
            }


        }

        public async Task Insert<T>(T item)
        {
            var c = GetAsyncConnection();
            await c.InsertAsync(item);
        }

        public async Task Delete<T>(T item)
        {
            var c = GetAsyncConnection();
            await c.DeleteAsync(item);
        }

        public async Task Save<T>(T item, ESyncStatus syncStatus = ESyncStatus.Success, Exception exception = null, bool update = false) where T : Entity, new()
        {
            var c = GetAsyncConnection();

            item.SyncStatus = (byte)syncStatus;
          ///  item.UpdateDate = DateTime.Now;
            
            if (exception != null)
            {
                item.ErrorMessage = exception.Message;
                item.HasError = true;
                //var error = new SyncError()
                //{
                //    Date = DateTime.Now,Id = Guid.NewGuid(),Message = exception.Message
                //};

                //item.SyncErrorId = error.Id;

                //await c.InsertAsync(error);
            }
            else
            {
                item.ErrorMessage = "";
                item.HasError = false;
            }

            if (!update && (await c.FindAsync<T>(item.Id)) == null)
            {
                await c.InsertAsync(item);
            }
            else
            {
                await c.UpdateAsync(item);

            }
        }

        //todo: Ulas added additional filters CustomerSurveyID, SurveyType and Question_Ref == * 
        public async Task<List<SurvelemMap>> GetSurvelemMaps(string questionRef,string surveyId, string surveyType)
        {
            var c = GetAsyncConnection();

            return await c.Table<SurvelemMap>().Where(x => (x.Question_Ref == questionRef || x.Question_Ref == "*") && x.CustomerSurveyID == surveyId && x.SurveyType.ToLower().Contains(surveyType.ToLower()) ).OrderBy(x=>x.Order).ToListAsync();
        }


        

        public async Task<List<Address>> GetAdresses(Customer customer)
        {
            var c = GetAsyncConnection();

            return await c.Table<Address>().Where(x => x.CustomerSurveyID == customer.CustomerSurveyID).OrderBy(x => x.UPRN).ToListAsync();
        }

        public async Task<List<Address>> GetCompletedANnIsLoadToPhoneAdresses()
        {
            var c = GetAsyncConnection();

            return await c.Table<Address>().Where(x => x.IsCompleted && x.IsLoadToPhone).ToListAsync();
        }



        public async Task<List<SurveyType>> GetSurveyTypes(string surveyId)
        {
            var c = GetAsyncConnection();

            return await c.Table<SurveyType>().Where(x=>x.CustomerSurveyID == surveyId).ToListAsync();
        }

        public async Task<List<Customer>> GetCustomers()
        {
            var c = GetAsyncConnection();

            return await c.Table<Customer>().ToListAsync();
        }

//TODO: Added by Ulas, for filtering Questions by customer survey ID, need to check if it breaks anything.Added surveyID
        public async Task<List<Option>> GetFirstLevelOptions(string questionRef, string surveyId)
        {
            var c = GetAsyncConnection();

            return await c.Table<Option>().Where(x => x.Question_Ref == questionRef && x.Level == "1" && x.CustomerSurveyID == surveyId).OrderBy(x => x.Option_Number).ToListAsync();
        }

//TODO: Added by Ulas, for filtering Questions by customer survey ID, need to check if it breaks anything.Added surveyID
        public async Task<List<Option>> GetSecondLevelOptions(string questionRef, string surveyId)
        {
            var c = GetAsyncConnection();

            return await c.Table<Option>().Where(x => x.Question_Ref == questionRef && x.Level == "2" && x.CustomerSurveyID == surveyId).OrderBy(x => x.Option_Number).ToListAsync();
        }

        //public async Task<QuestionStatus> FindQuestionStatus(string qRef, string uprn)
        //{
        //    return
        //        await
        //            GetAsyncConnection().Table<QuestionStatus>().Where(x => x.QuestionRef == qRef && x.UPRN == uprn).FirstOrDefaultAsync();
        //}





        public async Task<AddressStatus> FindAddressStatus(Guid addressId)
        {
            return
                await
                    GetAsyncConnection().Table<AddressStatus>().Where(x => x.AddressId == addressId).FirstOrDefaultAsync();
        }

        public async Task<Address> FindAddress(Guid addressId)
        {
            return
                await
                    GetAsyncConnection().Table<Address>().Where(x => x.Id == addressId).FirstOrDefaultAsync();
        }


        public async Task<Question> FindQuestion(string qRef, string custSurvId)
        {
            return
                await
                    GetAsyncConnection().Table<Question>().Where(x => x.Question_Ref == qRef && x.CustomerSurveyID == custSurvId).FirstOrDefaultAsync();
        }


        public async Task<Survelem> FindAnswer(string qRef, string uprn)
        {
            return
                await
                    GetAsyncConnection().Table<Survelem>().Where(x => x.Question_Ref == qRef && x.UPRN == uprn && x.IsDeletedOnClient == false).FirstOrDefaultAsync();
        }

        public async Task<List<AddressQuestionGroupStatus>> GetAddressQuestionGroups(Guid addressId)
        {
            var r = await
                GetAsyncConnection()
                    .Table<AddressQuestionGroupStatus>()
                    .Where(x => x.AddressId == addressId)
                    .ToListAsync();

            return r;
        }



        public async Task<AddressQuestionGroupStatus> FindAddressQuestionGroupStatus(Guid addressId, string name)
        {
            var r = await
                    GetAsyncConnection().Table<AddressQuestionGroupStatus>().Where(x => x.AddressId == addressId && x.Group == name && x.IsDeletedOnClient == false).FirstOrDefaultAsync();

            return r;
        }

        public async Task<bool> FindIfQuestionGroupIsCompleted(Guid addressId, string name)
        {
            var r = await FindAddressQuestionGroupStatus(addressId, name);

            return r != null;
        }

        public async Task<bool> FindIfQuestionIsCompleted(string qRef, string uprn)
        {
            var r = await
                FindAnswer(qRef, uprn);

            return r != null;
        }


        //public async Task<bool> FindIsQuestionGroupCompleted(string questionGroup,string uprn)
        //{
        //    return new ApplicationSettingsService().GetSetting(uprn + "." + questionGroup, false);

        //    //var c = GetAsyncConnection();

        //    //foreach (var question in questions)
        //    //{
        //    //    var status = await FindQuestionStatus(question.Question_Ref,uprn);

        //    //    if (status != null && status.Completed)
        //    //    {
        //    //        //
        //    //    }
        //    //    else
        //    //    {
        //    //        return false;
        //    //    }
        //    //}
        //    //return true;
        //}

        public async Task<RichMedia> FindMedia(string questionRef, string uprn)
        {
            return await GetAsyncConnection().Table<RichMedia>().Where(x => x.Question_Ref == questionRef && x.UPRN == uprn && x.IsDeletedOnClient == false).FirstOrDefaultAsync();
        }

        //public async Task<SurvelemSecond> FindSurvelemSecond(Guid id, string name)
        //{
        //    return await GetAsyncConnection().Table<SurvelemSecond>().Where(x => x.SurvelemId == id && x.Name == name).FirstOrDefaultAsync();
        //}


        public async Task ClearDb()
        {

            var c = GetAsyncConnection();

            await ClearTable<Surveyor>(c);


            var notSyncedAddresses = await GetNotSyncedEntitiesCount<Address>();

            if (notSyncedAddresses == 0)
            {
                await ClearTable<Address>(c);
            }
            else
            {
                var addreses = await GetSyncedEntities<Address>();

                await c.DeleteListAsync(addreses.Cast<object>().ToList());

            }


            var addressStatuses = await GetSyncedEntities<AddressStatus>();
            await c.DeleteListAsync(addressStatuses.Cast<object>().ToList());


            var richMedias = await GetSyncedEntities<RichMedia>();
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var r in richMedias)
                {
                    if (!iso.FileExists(r.FileName)) continue;

                    iso.DeleteFile(r.FileName);
                }
            }

            await c.DeleteListAsync(richMedias.Cast<object>().ToList());

            var qaaddreses = await GetSyncedEntities<QAAddress>();

            await c.DeleteListAsync(qaaddreses.Cast<object>().ToList());


            var qaaddresesComments = await GetSyncedEntities<QAAddressComment>();

            await c.DeleteListAsync(qaaddresesComments.Cast<object>().ToList());


            var addressQuestionGroupStatuses = await GetSyncedEntities<AddressQuestionGroupStatus>();

            await c.DeleteListAsync(addressQuestionGroupStatuses.Cast<object>().ToList());

            await ClearTable<Question>(c);
            await ClearTable<Option>(c);
            await ClearTable<SurveyType>(c);
            await ClearTable<Customer>(c);
            await ClearTable<SurvelemMap>(c);


            var notSyncedSurvelems = await GetNotSyncedEntitiesCount<Survelem>();

            if (notSyncedSurvelems == 0)
            {
                await ClearTable<Survelem>(c);
            }
            else
            {
                var survelems = await GetSyncedEntities<Survelem>();

                await c.DeleteListAsync(survelems.Cast<object>().ToList());
            }



            


       
        }

        Task<List<T>> GetSyncedEntities<T>() where T : Entity, new()
        {
            return GetAsyncConnection().Table<T>().Where(x => x.SyncStatus == (byte)ESyncStatus.Success).ToListAsync();
        }
        
        public void Initialize()
        {
            using (var c = GetConnection())
            {
               c.Execute("PRAGMA page_size = 65536 ;");
                
                c.CreateTable<Surveyor>();
                c.CreateTable<Customer>();
                c.CreateTable<Address>();
               c.CreateTable<Question>();
                c.CreateTable<Option>();
                c.CreateTable<RichMedia>();
                c.CreateTable<Survelem>();
                c.CreateTable<SurveyType>();
                c.CreateTable<SurvelemMap>();
               c.CreateTable<AddressStatus>();
               c.CreateTable<QAAddress>();
                c.CreateTable<QAAddressComment>();

                c.CreateTable<AddressQuestionGroupStatus>();


                //TODO: think about it
                //try
                //{
                //    c.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UPRN_Question_Ref_Unique ON Survelem (UPRN,Question_Ref)");
                //}
                //catch (Exception)
                //{
                    
                //}
            }
        }
    }
}

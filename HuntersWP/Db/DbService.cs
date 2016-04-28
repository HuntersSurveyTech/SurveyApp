using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using HuntersWP.Models;
using HuntersWP.Services;
using SQLite;

namespace HuntersWP.Db
{
    public class DbService
    {
        public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "db1.sqlite"));

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

        public async Task<List<Address>> GetAddressesForCopyTo(string type)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<Address>()
                        .Where(x => x.Complete == false && x.CopyTo == true && x.Type == type) //bitince x.copyTo=true'ya cevir
                        .ToListAsync();
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

        public async Task<List<RichMedia>> GetRichMediasAddressUPRN(string uprn)
        {
            return
                await
                    GetAsyncConnection()
                        .Table<RichMedia>()
                        .Where(x => x.UPRN == uprn)
                        .ToListAsync();
        }

        public async Task<List<T>> GetNotSyncedEntities<T>() where T:Entity,new()
        {
            return await GetAsyncConnection().Table<T>().Where(x => x.SyncStatus == (byte)ESyncStatus.NotSynced || x.SyncStatus == (byte)ESyncStatus.InProcess).ToListAsync();
        }

        public async Task ClearTable<T>() where T : new()
        {
            var c = GetAsyncConnection();

            await c.DropTableAsync<T>();
            await c.CreateTableAsync<T>();
        }


        public async Task<List<Question>> GetQuestions(string surveyTypesName)
        {
            var questions = await GetAsyncConnection().Table<Question>().ToListAsync();

            var surveyTypes =
                surveyTypesName.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries)
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


            return r.OrderBy(x=>x.Question_Order).ToList();


        }



        public async Task<int> Count<T>() where T:new()
        {
            return await GetAsyncConnection().Table<T>().CountAsync();
        }


        public async Task<SurveyTypes> FindSurveyType(string name)
        {
            return await GetAsyncConnection().Table<SurveyTypes>().Where(x => x.Name == name).FirstOrDefaultAsync();
        }


        public async Task<T> Find<T>(object id) where T : Entity, new()
        {
            return await GetAsyncConnection().FindAsync<T>(id);
        }

        public async Task Insert<T>(List<T> items) where T : Entity
        {
            foreach (var item in items)
            {
                item.SyncStatus = (byte) ESyncStatus.Success;
            }

            var c = GetAsyncConnection();
            await c.InsertAllAsync(items);
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

        public async Task Save<T>(T item,ESyncStatus syncStatus = ESyncStatus.Success,Exception exception = null, bool update = false)  where  T:Entity,new()
        {
            var c = GetAsyncConnection();

            item.SyncStatus = (byte)syncStatus;

            if(exception != null)
            {
                var error = new SyncError()
                {
                    Date = DateTime.Now,Identity = Guid.NewGuid().ToString(),Message = exception.Message
                };

                item.SyncErrorId = error.Identity;

                await c.InsertAsync(error);
            }

            if (!update && (await c.FindAsync<T>(item.Identity)) == null)
            {
                await c.InsertAsync(item);
            }
            else
            {
                await c.UpdateAsync(item);

            }
        }

        public async Task<List<SurvelemMap>> GetSurvelemMaps(string questionRef)
        {
            var c = GetAsyncConnection();

            return await c.Table<SurvelemMap>().Where(x => x.Question_Ref == questionRef).OrderBy(x=>x.SvmMapID).ToListAsync();
        }


        

        public async Task<List<Address>> GetAdresses(Customer customer)
        {
            var c = GetAsyncConnection();

            return await c.Table<Address>().Where(x=>x.CustomerSurveyID == customer.CustomerSurveyID).OrderBy(x=>x.AddressID).ToListAsync();
        }


        public async Task<List<SurveyTypes>> GetSurveyTypes()
        {
            var c = GetAsyncConnection();

            return await c.Table<SurveyTypes>().ToListAsync();
        }

        public async Task<List<Customer>> GetCustomers()
        {
            var c = GetAsyncConnection();

            return await c.Table<Customer>().ToListAsync();
        }


        public async Task<List<Option>> GetFirstLevelOptions(string questionRef)
        {
            var c = GetAsyncConnection();

            return await c.Table<Option>().Where(x => x.Question_Ref == questionRef && x.Level == "1").OrderBy(x => x.Option_Number).ToListAsync();
        }

        public async Task<List<Option>> GetSecondLevelOptions(string questionRef)
        {
            var c = GetAsyncConnection();

            return await c.Table<Option>().Where(x => x.Question_Ref == questionRef && x.Level == "2").OrderBy(x => x.Option_Number).ToListAsync();
        }

        public async Task<QuestionStatus> FindQuestionStatus(string qRef, string uprn)
        {
            return
                await
                    GetAsyncConnection().Table<QuestionStatus>().Where(x => x.QuestionRef == qRef && x.UPRN == uprn).FirstOrDefaultAsync();
        }

        public async Task<Question> FindQuestion(string qRef)
        {
            return
                await
                    GetAsyncConnection().Table<Question>().Where(x => x.Question_Ref == qRef).FirstOrDefaultAsync();
        }

        public async Task<Survelem> FindAnswer(string qRef, string uprn)
        {
            return
                await
                    GetAsyncConnection().Table<Survelem>().Where(x => x.Question_Ref == qRef && x.UPRN == uprn).FirstOrDefaultAsync();
        }


        public async Task<bool> FindIsQuestionGroupCompleted(string questionGroup,string uprn)
        {
            return new ApplicationSettingsService().GetSetting(uprn + "." + questionGroup, false);

            //var c = GetAsyncConnection();

            //foreach (var question in questions)
            //{
            //    var status = await FindQuestionStatus(question.Question_Ref,uprn);

            //    if (status != null && status.Completed)
            //    {
            //        //
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //return true;
        }

        public async Task<RichMedia> FindMedia(string questionRef, string uprn)
        {
            return await GetAsyncConnection().Table<RichMedia>().Where(x => x.Question_Ref == questionRef && x.UPRN == uprn).FirstOrDefaultAsync();
        }


        public async Task ClearDb()
        {
            await ClearTable<Surveyor>();
            await ClearTable<Survey>();
            //await ClearTable<Address>();

            var c = GetAsyncConnection();

            var addreses = await GetEntitiesWithoutErrorSynced<Address>();

            await c.DeleteListAsync(addreses.Cast<object>().ToList());

            var richMedias = await GetEntitiesWithoutErrorSynced<RichMedia>();
            await c.DeleteListAsync(richMedias.Cast<object>().ToList());


            var survelems = await GetEntitiesWithoutErrorSynced<Survelem>();

            await c.DeleteListAsync(survelems.Cast<object>().ToList());

            await ClearTable<SyncError>();
            await ClearTable<QuestionStatus>();
            await ClearTable<Question>();
            await ClearTable<Option>();
            //await ClearTable<RichMedia>();
            //await ClearTable<Survelem>();
            await ClearTable<SurveyTypes>();
            await ClearTable<Customer>();
            await ClearTable<SurvelemMap>();
        }

        Task<List<T>> GetEntitiesWithoutErrorSynced<T>() where T:Entity,new()
        {
            return GetAsyncConnection().Table<T>().Where(x => x.SyncStatus != (byte) ESyncStatus.Error).ToListAsync();
        }

        public void Initialize()
        {
            using (var c = GetConnection())
            {
                c.CreateTable<Surveyor>();
                //c.CreateTable<NetmeraUser>();
                c.CreateTable<Customer>();
                c.CreateTable<Survey>();
                c.CreateTable<Address>();
                c.CreateTable<SyncError>();
                c.CreateTable<Question>();
                c.CreateTable<Option>();
                c.CreateTable<RichMedia>();
                c.CreateTable<Survelem>();
                c.CreateTable<SurveyTypes>();
                c.CreateTable<SurvelemMap>();
                c.CreateTable<QuestionStatus>();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using HuntersWP.Db;
using HuntersWP.Models;
using Netmera;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HuntersWP.Services
{
   public class MyNetmeraClient
    {
       //public async Task<List<T>> GetObjects<T>(string objectName)
       //{


           
       //    return SearchObjects<>()
       //}

       public async Task<ApiResponse<string>> Update<T>(NetmeraContent content,string id)
       {
           if (content == null)
           {
               return new ApiResponse<string>() { IsSuccess = true };
           }

           bool setted = false;

           TaskCompletionSource<ApiResponse<string>> _task = new TaskCompletionSource<ApiResponse<string>>();

           try
           {
               content.update((c, ex) =>
               {
                   if (ex != null)
                   {
                       BugSense.BugSenseHandler.Instance.LogException(ex, "Entity", typeof(T).Name);
                       if (setted) return;
                       setted = true;
                       _task.SetResult(new ApiResponse<string>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = ex,Data = id});
                   }
                   else
                   {
                       if (setted) return;
                       setted = true;
                       _task.SetResult(new ApiResponse<string>() { IsSuccess = true,Data = id});
                   }
               });


               //await Task.Delay(1000);

               //if (!setted)
               //{
               //    setted = true;
               //    _task.SetResult(new ApiResponse<T>() { IsSuccess = true });
               //}
           }
           catch (Exception ex)
           {
               BugSense.BugSenseHandler.Instance.LogException(ex, "Entity", typeof(T).Name);
               _task.SetResult(new ApiResponse<string>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = ex,Data = id});
           }

     

           return await _task.Task;
       }

       List<Tuple<string, JObject>> ConvertToJObjects(List<Dictionary<string, string>> entities)
       {
           var contentJArray = new List<Tuple<string, JObject>>();
           foreach (var entity in entities)
           {
               var contentJSon = new JObject();
               foreach (var e in entity.Keys)
               {
                   contentJSon.Add(new JProperty(e, entity[e]));
               }

               contentJArray.Add(new Tuple<string, JObject>(entity["id"], contentJSon));
           }

           return contentJArray;
       }

       private readonly int BULK_QANTITY = 50;
       public async Task<ApiResponse<bool>> BatchSave(string table,
           List<Dictionary<string, string>> entities,
           List<Survelem> items,
           bool create)
       {

           var paramsJson = new JObject();

           //foreach (var entity in entities)
           //{
           //    var contentJSon = new JObject();
           //    foreach (var e in entity.Keys)
           //    {
           //        contentJSon.Add(new JProperty(e, entity[e]));
           //    }
               
           //    contentJArray.Add(new Tuple<string, JObject>(entity["id"], contentJSon));
           //}
           bool error = false;
           Exception exception = null;
           var j = 0;
           foreach (var e in entities.Batch(BULK_QANTITY))
           {
               var ss = ConvertToJObjects(e.ToList());

               var s = ss.ToList();
               
               var netmeraTable = new NetmeraContent(table);
               var bodyJArray = new JArray();
               int i = 0;
               foreach (var sik in s)
               {
                   j++;
                   if (create)
                   {
                       var o = sik.Item2;
                       o.Add("netmera-mobimera:api-content-type", "Survelem");

                       bodyJArray.Add(o);    
                   }
                   else
                   {
                       var query = new JObject();
                       var queryInner = new JObject();
                       queryInner.Add(new JProperty("netmera-mobimera:api-content-type", "Survelem"));
                       queryInner.Add(new JProperty("id", sik.Item1));

                       query.Add(new JProperty("data", sik.Item2));
                       query.Add(new JProperty("query",queryInner));
                       bodyJArray.Add(query);
                       Debug.WriteLine("Survelem added in the batch :" + sik.Item1);
                   }
                   i++;
               }
               if (create)
               {
                   paramsJson.Add("content", bodyJArray);
                   netmeraTable.add("params", paramsJson);
               }
               else
               {
                   netmeraTable.add("content", bodyJArray);
               }
   
               netmeraTable.add("method", create ? "content.createBulkContentWithoutActionToken" : "content.queryAndUpdateBulkContentWithoutActionToken");
               netmeraTable.add("st", App.KEY);

               var t = new TaskCompletionSource<object>();


               Action<JObject,Exception> a= async (JObject o, Exception ex) =>
               {
                  

                   paramsJson.RemoveAll();

                   foreach (var tuple in s)
                   {
                       var item = items.FirstOrDefault(x => x.id == tuple.Item1);
                       if (item != null)
                       {
                           Debug.WriteLine("Does Survelem exists in Netmera id:" + item.id);
                           NetmeraService service = new NetmeraService("Survelem");
                           service.whereEqual("id", item.id);

                           var survelem = await SearchObjects<Survelem>(service);

                           if (survelem.IsSuccess && survelem.Data.Any())
                           {
                               Debug.WriteLine("Survelem is in Netmera :" + item.id);
                               item.IsCreatedOnClient = false;
                               await
                                   new DbService().Save(item, ex == null ? ESyncStatus.Success : ESyncStatus.Error, ex, true);
                           }
                           else
                           {
                               var retryInsert = new NetmeraContent("Survelem");

                               retryInsert.add("COMMENT", item.COMMENT);
                               retryInsert.add("CustomerID", item.CustomerID.ToString());
                               retryInsert.add("CustomerSurveyID", item.CustomerSurveyID);
                               retryInsert.add("OptionID", item.OptionID ?? "");
                               retryInsert.add("OptionID2ndry", item.OptionID2ndry ?? "");
                               retryInsert.add("Question_Ref", item.Question_Ref);
                               retryInsert.add("UPRN", item.UPRN);
                               retryInsert.add("id", item.id);
                               retryInsert.add("Freetext", item.Freetext ?? "");
                               retryInsert.add("BuildingType", item.BuildingType ?? "");
                               retryInsert.add("DateOfSurvey", item.DateOfSurvey);

                               retryInsert.add("SqN1", item.SqN1 ?? "");
                               retryInsert.add("SqN10", item.SqN10 ?? "");
                               retryInsert.add("SqN11", item.SqN11 ?? "");
                               retryInsert.add("SqN12", item.SqN12 ?? "");
                               retryInsert.add("SqN13", item.SqN13 ?? "");
                               retryInsert.add("SqN14", item.SqN14 ?? "");
                               retryInsert.add("SqN15", item.SqN15 ?? "");
                               retryInsert.add("SqN2", item.SqN2 ?? "");
                               retryInsert.add("SqN3", item.SqN3 ?? "");
                               retryInsert.add("SqN4", item.SqN4 ?? "");
                               retryInsert.add("SqN5", item.SqN5 ?? "");
                               retryInsert.add("SqN6", item.SqN6 ?? "");
                               retryInsert.add("SqN7", item.SqN7 ?? "");
                               retryInsert.add("SqN8", item.SqN8 ?? "");
                               retryInsert.add("SqN9", item.SqN9 ?? "");
                               retryInsert.add("SqT1", item.SqT1 ?? "");
                               retryInsert.add("SqT10", item.SqT10 ?? "");
                               retryInsert.add("SqT11", item.SqT11 ?? "");
                               retryInsert.add("SqT12", item.SqT12 ?? "");
                               retryInsert.add("SqT13", item.SqT13 ?? "");
                               retryInsert.add("SqT14", item.SqT14 ?? "");
                               retryInsert.add("SqT15", item.SqT15 ?? "");
                               retryInsert.add("SqT2", item.SqT2 ?? "");
                               retryInsert.add("SqT3", item.SqT3 ?? "");
                               retryInsert.add("SqT4", item.SqT4 ?? "");
                               retryInsert.add("SqT5", item.SqT5 ?? "");
                               retryInsert.add("SqT6", item.SqT6 ?? "");
                               retryInsert.add("SqT7", item.SqT7 ?? "");
                               retryInsert.add("SqT8", item.SqT8 ?? "");
                               retryInsert.add("SqT9", item.SqT9 ?? "");



                               var r = await new MyNetmeraClient().Create<Survelem>(retryInsert, item.id);

                               if (r.IsSuccess)
                               {
                                   await new DbService().Save(item, ESyncStatus.Success, null, true);
                               }
                               else
                               {
                                   await new DbService().Save(item, ESyncStatus.Error, r.Exception, true);
                               }

                               Debug.WriteLine("Survelem Record not found, id is: " + item.id);
                               //item.IsCreatedOnClient = false;
                               //await new DbService().Save(item, ESyncStatus.Error, ex, true);

                           }

                       }
                   }

                   if (ex == null)
                   {
                       /////   
                   }
                   else
                   {
                       //error = true;
                       exception = ex;
                       Debug.WriteLine("BatchSave: "  + ex.Message);
                   }

                   t.SetResult(null);
               };

               //try
               //{

               if (create)
               {
                   netmeraTable.bulkCreate(async (o, ex) =>
                   {
                       a(o, ex);

                   });
               }
               else
               {
                   netmeraTable.bulkUpdate(async (o, ex) =>
                   {
                       a(o, ex);

                   });
               }


               await t.Task;

               if (error)
               {
                   return new ApiResponse<bool>() {Data = false, Exception = exception};
               }

           }

           return new ApiResponse<bool> { Data = true};

       }

       //public async Task<ApiResponse<List<List<string>>>> BatchUpdate(string table, List<Dictionary<string, string>> entities)
       //{

       //    var r = new List<List<string>>();

       //    var contentJArray = new JArray();
       //    var paramsJson = new JObject();

       //    foreach (var entity in entities)
       //    {
       //        var contentJSon = new JObject();
       //        foreach (var e in entity.Keys)
       //        {
       //            contentJSon.Add(new JProperty(e, entity[e]));
       //        }

       //        contentJArray.Add(contentJSon);
       //    }

       //    foreach (IEnumerable<JToken> s in contentJArray.Children().Batch(BULK_QANTITY))
       //    {

       //        var l = new List<String>();

       //        var netmeraTable = new NetmeraContent(table);
       //        var bodyJArray = new JArray();
       //        foreach (JToken sik in s)
       //        {
       //            bodyJArray.Add(sik);
       //        }
       //        paramsJson.Add("content", bodyJArray);
       //        netmeraTable.add("params", paramsJson);
       //        netmeraTable.add("method", "content.updateBulkContentWithoutActionToken");
       //        netmeraTable.add("st", App.KEY);

       //        var t = new TaskCompletionSource<object>();

       //        //try
       //        //{
       //        netmeraTable.bulkUpdate((o, ex) =>
       //        {
       //            t.SetResult(null);

       //            paramsJson.RemoveAll();
       //        });
       //        //}
       //        //catch (NetmeraException ex)
       //        //{
       //        //    if (ex.getCode().ToString() != "135")
       //        //    { MessageBox.Show(ex.getCode().ToString()); }
       //        //}

       //        await t.Task;

       //    }

       //    return new ApiResponse<List<List<string>>> { Data = r };

       //}

       public async Task<ApiResponse<string>> Create<T>(NetmeraContent content, string id)
       {
           bool setted = false;
           TaskCompletionSource<ApiResponse<string>> _task = new TaskCompletionSource<ApiResponse<string>>();

           try
           {

               content.create((c, ex) =>
               {
                   if (ex != null)
                   {
                       if (setted) return;
                       setted = true;
                       BugSense.BugSenseHandler.Instance.LogException(ex,"Entity",typeof(T).Name);
                       _task.SetResult(new ApiResponse<string>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = ex, Data = id });
                   }
                   else
                   {
                       if (setted) return;
                       setted = true;
                       _task.SetResult(new ApiResponse<string>() { IsSuccess = true,Data = id});
                   }
               });

               //await Task.Delay(1000);

               //if (!setted)
               //{
               //    setted = true;
               //    _task.SetResult(new ApiResponse<T>() { IsSuccess = true });
               //}
           }
           catch (Exception ex)
           {
               BugSense.BugSenseHandler.Instance.LogException(ex, "Entity", typeof(T).Name);
               _task.SetResult(new ApiResponse<string>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = ex, Data = id });
           }



           return await _task.Task;
       }


       async Task<ApiResponse<List<T>>> SearchObjects<T>(NetmeraService service)
       {
           TaskCompletionSource<ApiResponse<List<T>>> _task = new TaskCompletionSource<ApiResponse<List<T>>>();
           service.setMax(5000);

           service.search((list, exception) =>
           {
               if (exception != null)
               {
                   _task.SetResult(new ApiResponse<List<T>>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = exception });
               }
               else
               {
                   var objects = new List<T>();

                   foreach (var netmeraContent in list)
                   {
                       var o = JsonConvert.DeserializeObject<T>(netmeraContent.data.ToString());

                       objects.Add(o);
                   }

                   _task.SetResult(new ApiResponse<List<T>>(){Data = objects,IsSuccess = true});

               }

           });

           return  await _task.Task;

           
       }

       public async Task<long> Count<T>(NetmeraService service)
       {
           var task = new TaskCompletionSource<long>();

        if (typeof (T) == typeof (Models.Address))
           {
               service.setSortBy("UPRN");
           }
           else if (typeof (T) == typeof (Models.Survelem))
           {
               service.setSortBy("id");
           }
           else if (typeof (T) == typeof (Models.Question))
           {
               service.setSortBy("Question_Order");
           }
           else
           {
               service.setSortBy("Path");
           }

           service.setSortOrder(NetmeraService.SortOrder.ascending);
           service.setMax(1);
           service.count((c, e) =>
           {
               if (e != null)
               {
                   task.SetResult(-1);
               }
               else
               {
                   task.SetResult(c);
               }


           });

           return await task.Task;
       }


       async Task<ApiResponse<List<T>>> SearchObjectsWithPaging<T>(NetmeraService service)
       {
           object locker = new object();
           TaskCompletionSource<ApiResponse<List<T>>> _task = new TaskCompletionSource<ApiResponse<List<T>>>();
           var page = 100;
           //service.setMax(page);
           if (typeof (T) == typeof (Models.Address))
           {
               service.setSortBy("UPRN");
           }
           else if (typeof (T) == typeof (Models.Survelem))
           {
               service.setSortBy("id");
           }
           else if (typeof (T) == typeof (Models.Question))
           {
               service.setSortBy("Question_Order");
           }
           else
           {
               service.setSortBy("Path");
           }

           service.setSortOrder(NetmeraService.SortOrder.ascending);
           service.count((c, e) =>
           {

              

               if (e != null)
               {
                   _task.SetResult(new ApiResponse<List<T>>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = e });
                   return;
               }
               else
               {

                   if (c == 0)
                   {
                       _task.SetResult(new ApiResponse<List<T>>() { Data = new List<T>(), IsSuccess = true });
                       return;
                   }
                   else
                   {
                       bool setResult = false;
                       int completed = -1;
                       var pages = c / page;
                       var objects = new List<T>();
                       for (int i = 0; i <= pages; i++)
                       {
                           service.setPage(i);
                           service.setMax(page);
                           service.search((list, exception) =>
                           {
                               if (exception != null)
                               {

                                   lock (locker)
                                   {
                                       if (!setResult)
                                       {
                                           setResult = true;
                                           _task.SetResult(new ApiResponse<List<T>>()
                                           {
                                               IsSuccess = false,
                                               Message = "Error while processing request. Try again",
                                               Exception = exception
                                           });
                                  
                                       }
                                   }
                               }
                               else
                               {
                                
                                   lock (locker)
                                   {
                                       foreach (var netmeraContent in list)
                                       {
                                           var o = JsonConvert.DeserializeObject<T>(netmeraContent.data.ToString());

                                           objects.Add(o);
                                       }

                                       completed++;

                                       if (completed == pages)
                                       {
                                           setResult = true;
                                           _task.SetResult(new ApiResponse<List<T>>() { Data = objects, IsSuccess = true });
                                           
                                       }
                                   }


                               }

                           });
                       }
                   }

                 

       
               }
           });

          

           return await _task.Task;


       }

       async Task<ApiResponse<NetmeraContent>> SearchNetmeraContent(NetmeraService service)
       {
           TaskCompletionSource<ApiResponse<NetmeraContent>> _task = new TaskCompletionSource<ApiResponse<NetmeraContent>>();
           service.setMax(1);
           service.search((list, exception) =>
           {
               if (exception != null)
               {
                   _task.SetResult(new ApiResponse<NetmeraContent>() { IsSuccess = false, Message = "Error while processing request. Try again", Exception = exception });
               }
               else
               {
                   if (list.Any())
                   {
                       _task.SetResult(new ApiResponse<NetmeraContent>() { Data = list[0], IsSuccess = true }); 
                   }
                   else
                   {
                       _task.SetResult(new ApiResponse<NetmeraContent>() { IsSuccess = false, Message = "Object was not found"});
                   }
                  

               }

           });

           return await _task.Task;


       }

       public async Task<NetmeraContent> FindNetmeraContent(string table, string idField, object idValue)
       {
           
           NetmeraService service = new NetmeraService(table);
           service.whereEqual(idField, idValue);

           var items = await SearchNetmeraContent(service);

           if (items.IsSuccess)
           {
               return items.Data;
           }
           
           return null;
       }

       public async Task<ApiResponse<List<T>>> FindNetmeraContents<T>(string table, string idField, List<string> idValues)
       {
           NetmeraService service = new NetmeraService(table);
           service.whereContainedIn(idField,idValues);

           var items = await SearchObjects<T>(service);

           return items;
       }

      // public async Task<List<Customer>> GetAllCustomers()
      // {

      //    // var r1 = await GetObjects<Address>("NetmeraUser");

      //     return await GetObjects<Customer>("Customers");

      //}


       public async Task<ApiResponse<List<Surveyor>>> Login(string login, string password)
       {
           NetmeraService service = new NetmeraService("Surveyors");
           service.whereEqual("Username", login);
           service.whereEqual("Password", Helpers.Sha1(password));


           var surveyors = await SearchObjects<Surveyor>(service);

           if (surveyors.IsSuccess && surveyors.Data.Any())
           {
               return surveyors;
           }
           else
           {
               if (string.IsNullOrEmpty(surveyors.Message))
               {
                   surveyors.Message = "Wrong username or password";
               }

               surveyors.IsSuccess = false;

               return surveyors;
           }



       }

       public void IsNeedToWipeDevice()
       {
           
       }

       public void SendErrors()
       {
           
       }

       //public void GetAddresses()
       //{
           
       //}

       //public void GetQuestions()
       //{
           
       //}

       public async Task<List<Customer>> GetCustomers(long userId)
       {
           var service1 = new NetmeraService("Customers");
           service1.whereEqual("Completed", "False");

           var customers = await SearchObjects<Customer>(service1);

           var foundCustomers = new List<Customer>();
           if (customers.IsSuccess)
           {
               foreach (var customer in customers.Data.Where(x => !x.Completed))
               {
                   var ids = customer.Surveyors.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                   if (ids.Contains(userId.ToString()))
                   {
                       foundCustomers.Add(customer);

                   }
               }
           }
           return foundCustomers;
       }

       public async Task<ApiResponse<Address>> SyncUserData(long userId)
       {
           ApiResponse<Address> r = null;
           try
           {
               r =  await ExecuteSyncUserData(userId);
           }
           catch (Exception)
           {
              r = new ApiResponse<Address>() {IsSuccess = false};
           }
           if (!r.IsSuccess)
           {
               await CleanDb();
           }
           return r;

       }

       async Task CleanDb()
       {
           await new DbService().ClearDb();
       }

       public async Task<ApiResponse<Address>> UpdateQuestionsAndOptions(long userId)
       {
           var foundCustomers = await GetCustomers(userId);

           await new DbService().ClearTable<Question>();
           await new DbService().ClearTable<Option>();

           var r = await DownloadQuestionsAndOptions(foundCustomers);

           return r;

       }

       async Task<ApiResponse<Address>> ExecuteSyncUserData(long userId)
       {
           var surveyTypes = await SearchObjects<SurveyTypes>(new NetmeraService("SurveyTypes"));

           if (surveyTypes.IsSuccess)
           {
               await new DbService().ClearTable<SurveyTypes>();

               foreach (var surveyTypese in surveyTypes.Data.Distinct())
               {
                   surveyTypese.Identity = surveyTypese.ID;
                   await new DbService().Save(surveyTypese);
               }

           }
           else
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }


           var foundCustomers = await GetCustomers(userId);

           foreach (var foundCustomer in foundCustomers)
           {
               foundCustomer.Identity = foundCustomer.CustomerID.ToString();
               await new DbService().Save(foundCustomer);
           }

           NetmeraService service2 = new NetmeraService("Address_List1");
           service2.whereEqual("Surveyor", userId.ToString());
           service2.whereEqual("Complete", false);


           //var addresses = await SearchObjects<Address>(service2);
           var addresses = await SearchObjectsWithPaging<Address>(service2);

           var downloadedAddresses = new List<Address>();
           if (addresses.IsSuccess)
           {
               var insert = new List<Address>();
               foreach (var address in addresses.Data)
               {
                   if (foundCustomers.Any(x => x.CustomerSurveyID.ToString() == address.CustomerSurveyID.ToString()))
                   {
                       address.Identity = address.AddressID;

                       if (string.IsNullOrEmpty(address.FullAddress))
                       {
                           address.FullAddress = "";
                       }

                       if (!insert.Any(x => x.Identity == address.Identity))
                       {
                           insert.Add(address);
                           downloadedAddresses.Add(address);
                       }


                   }
               }

               await new DbService().Insert(insert);

           }
           else
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }


           var r = await DownloadQuestionsAndOptions(foundCustomers);

           if (!r.IsSuccess)
           {
               return r;
           }
           //var questions = await SearchObjects<Question>(new NetmeraService("Questions"));
           //var questions = await SearchObjectsWithPaging<Question>(new NetmeraService("Questions"));

           //if (questions.IsSuccess)
           //{
           //    var insert = new List<Question>();
           //    foreach (var question in questions.Data)
           //    {
           //        if (foundCustomers.Any(x => x.CustomerSurveyID == question.CustomerSurveyID))
           //        {
           //            question.Identity = question.Question_Ref;
           //            if (!insert.Any(x => x.Identity == question.Identity))
           //                insert.Add(question);

           //        }
           //    }

           //    await new DbService().Insert(insert);
           //}
           //else
           //{
           //    return new ApiResponse<Address>() { IsSuccess = false };
           //}

           //var options = await SearchObjectsWithPaging<Option>(new NetmeraService("Options"));
           ////var options = await SearchObjects<Option>(new NetmeraService("Options"));

           //if (options.IsSuccess)
           //{
           //    var insert = new List<Option>();
           //    foreach (var option in options.Data)
           //    {
           //        if (foundCustomers.Any(x => x.CustomerSurveyID == option.CustomerSurveyID))
           //        {
           //            option.Identity = option.OptionId;
           //            if (!insert.Any(x => x.Identity == option.Identity))
           //                insert.Add(option);


           //        }
           //    }
           //    await new DbService().Insert(insert);
           //}
           //else
           //{
           //    return new ApiResponse<Address>() { IsSuccess = false };
           //}



           var media = await SearchObjects<RichMedia>(new NetmeraService("RichMedia"));

           if (media.IsSuccess)
           {
               var insert = new List<RichMedia>();
               foreach (var option in media.Data)
               {
                   if (foundCustomers.Any(x => x.CustomerSurveyID == option.CustomerSurveyID))
                   {
                       option.Identity = option.ID;
                       if (!insert.Any(x => x.Identity == option.Identity))
                           insert.Add(option);


                   }
               }
               await new DbService().Insert(insert);
           }
           else
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }

           var page = 100;
           for (int i = 0; i < downloadedAddresses.Count; i=i+page)
           {
               var survelemSearch = new NetmeraService("Survelem");

               survelemSearch.whereContainedIn("UPRN", downloadedAddresses.Skip(i).Take(page).Select(x => x.UPRN).ToList());

               //var survelems = await SearchObjects<Survelem>(survelemSearch);
               var survelems = await SearchObjectsWithPaging<Survelem>(survelemSearch);

               if (survelems.IsSuccess)
               {
                   var insert = new List<Survelem>();
                   foreach (var option in survelems.Data)
                   {
                       if (foundCustomers.Any(x => x.CustomerSurveyID == option.CustomerSurveyID))
                       {
                           option.Identity = option.id;
                           if (!insert.Any(x => x.Identity == option.Identity))
                               insert.Add(option);


                       }
                   }
                   await new DbService().Insert(insert);
               }
               else
               {
                   return new ApiResponse<Address>() { IsSuccess = false };
               }
           }

     



           var survelemMaps = await SearchObjects<SurvelemMap>(new NetmeraService("TblSurvelemMap"));

           if (survelemMaps.IsSuccess)
           {
               var insert = new List<SurvelemMap>();
               foreach (var option in survelemMaps.Data)
               {
                   if (foundCustomers.Any(x => x.CustomerSurveyID == option.CustomerSurveyID))
                   {
                       option.Identity = option.SvmMapID;
                       if (!insert.Any(x => x.Identity == option.Identity))
                           insert.Add(option);


                   }
               }
               await new DbService().Insert(insert);
           }
           else
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }

           return new ApiResponse<Address>() { IsSuccess = true };
       }

       async Task<ApiResponse<Address>>  DownloadQuestionsAndOptions(List<Customer> foundCustomers )
       {
            var questions = await SearchObjectsWithPaging<Question>(new NetmeraService("Questions"));

           if (questions.IsSuccess)
           {
               var insert = new List<Question>();
               foreach (var question in questions.Data)
               {
                   if (foundCustomers.Any(x => x.CustomerSurveyID == question.CustomerSurveyID))
                   {
                       question.Identity = question.Question_Ref;
                       if (!insert.Any(x => x.Identity == question.Identity))
                           insert.Add(question);

                   }
               }

               await new DbService().Insert(insert);
           }
           else
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }

           var netmeraCount = await Count<Question>(new NetmeraService("Questions"));

           if (netmeraCount == -1)
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }

           var dbCount = await new DbService().Count<Question>();

           if (netmeraCount != dbCount)
           {
               return new ApiResponse<Address>() { IsSuccess = false,Message = "Invalid count of Questions downloaded. Try again"};
           }

           var options = await SearchObjectsWithPaging<Option>(new NetmeraService("Options"));
           //var options = await SearchObjects<Option>(new NetmeraService("Options"));

           if (options.IsSuccess)
           {
               var insert = new List<Option>();
               foreach (var option in options.Data)
               {
                   if (foundCustomers.Any(x => x.CustomerSurveyID == option.CustomerSurveyID))
                   {
                       option.Identity = option.OptionId;
                       if (!insert.Any(x => x.Identity == option.Identity))
                           insert.Add(option);


                   }
               }
               await new DbService().Insert(insert);
           }
           else
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }

           netmeraCount = await Count<Question>(new NetmeraService("Options"));

           if (netmeraCount == -1)
           {
               return new ApiResponse<Address>() { IsSuccess = false };
           }

           dbCount = await new DbService().Count<Option>();
           if (netmeraCount != dbCount)
           {
               return new ApiResponse<Address>() { IsSuccess = false, Message = "Invalid count of Options downloaded. Try again" };
           }

           return new ApiResponse<Address>(){IsSuccess = true};
       }

    
    }

   static partial class MoreEnumerable
   {
       /// <summary>
       /// Batches the source sequence into sized buckets.
       /// </summary>
       /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
       /// <param name="source">The source sequence.</param>
       /// <param name="size">Size of buckets.</param>
       /// <returns>A sequence of equally sized buckets containing elements of the source collection.</returns>
       /// <remarks> This operator uses deferred execution and streams its results (buckets and bucket content).</remarks>
       public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
       {
           return Batch(source, size, x => x);
       }

       /// <summary>
       /// Batches the source sequence into sized buckets and applies a projection to each bucket.
       /// </summary>
       /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
       /// <typeparam name="TResult">Type of result returned by <paramref name="resultSelector"/>.</typeparam>
       /// <param name="source">The source sequence.</param>
       /// <param name="size">Size of buckets.</param>
       /// <param name="resultSelector">The projection to apply to each bucket.</param>
       /// <returns>A sequence of projections on equally sized buckets containing elements of the source collection.</returns>
       /// <remarks> This operator uses deferred execution and streams its results (buckets and bucket content).</remarks>
       public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size,
           Func<IEnumerable<TSource>, TResult> resultSelector)
       {
           if (source == null) throw new ArgumentNullException("source");
           if (size <= 0) throw new ArgumentOutOfRangeException("size");
           if (resultSelector == null) throw new ArgumentNullException("resultSelector");
           return BatchImpl(source, size, resultSelector);
       }

       private static IEnumerable<TResult> BatchImpl<TSource, TResult>(this IEnumerable<TSource> source, int size,
           Func<IEnumerable<TSource>, TResult> resultSelector)
       {
           Debug.Assert(source != null);
           Debug.Assert(size > 0);
           Debug.Assert(resultSelector != null);

           TSource[] bucket = null;
           var count = 0;

           foreach (var item in source)
           {
               if (bucket == null)
               {
                   bucket = new TSource[size];
               }

               bucket[count++] = item;

               // The bucket is fully buffered before it's yielded
               if (count != size)
               {
                   continue;
               }

               // Select is necessary so bucket contents are streamed too
               yield return resultSelector(bucket.Select(x => x));

               bucket = null;
               count = 0;
           }

           // Return the last bucket with all remaining elements
           if (bucket != null && count > 0)
           {
               yield return resultSelector(bucket.Take(count));
           }
       }
   }
}

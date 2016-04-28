using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using HuntersWP.Db;
using HuntersWP.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Address = HuntersWP.Models.Address;
using Customer = HuntersWP.Models.Customer;
using Option = HuntersWP.Models.Option;
using Question = HuntersWP.Models.Question;
using RichMedia = HuntersWP.Models.RichMedia;
using Survelem = HuntersWP.Models.Survelem;
using SurvelemMap = HuntersWP.Models.SurvelemMap;
using SurveyType = HuntersWP.Models.SurveyType;

namespace HuntersWP.Services
{
   public class MyNetmeraClient
    {
     
      public async Task<HuntersWP.ServiceReference.LoginReply> Login(string login, string password)
       {
           var reply = await new MyServiceClient().Login(login, Helpers.Sha1(password));

           if (!reply.IsSuccess)
           {
               if (string.IsNullOrEmpty(reply.Data))
               {
                   reply.Data = "Wrong username or password";
               }
           }

           return reply;
       }



       public async Task ClearDb()
       {
           await new DbService().ClearDb();
       }

       public async Task UpdateQuestionsAndOptions()
       {
           var foundCustomers = await new DataLoaderService().GetCustomers();

           await new DbService().ClearTable<Question>();
           await new DbService().ClearTable<Option>();

           await DownloadQuestionsAndOptionsAndSurvelemMaps(foundCustomers);
       }

       public async Task RefreshAddressesProperties()
       {
           var addresses = await new DbService().GetCompletedANnIsLoadToPhoneAdresses();

           foreach (var dbAd in addresses)
           {
               var loaded = await new DataLoaderService().GetAddresses(new List<Customer>(), dbAd.Id);

               if (loaded.Any())
               {
                   var ad = loaded.First();
                   if (ad != null)
                   {
                       dbAd.IsLoadToPhone = ad.IsLoadToPhone;
                       await new DbService().Save(dbAd, (ESyncStatus)dbAd.SyncStatus);
                   }

               }

           }

       }

       public async Task DownloadUserData()
       {
            var surveyTypes = await new DataLoaderService().GetSurveyTypes();

           await new DbService().ClearTable<SurveyType>();

           foreach (var surveyTypese in surveyTypes)
           {
               await new DbService().Save(surveyTypese);
           }
           
           var foundCustomers = await new DataLoaderService().GetCustomers();

           foreach (var foundCustomer in foundCustomers)
           {
               await new DbService().Save(foundCustomer);
           }

           var addresses = await new DataLoaderService().GetAddresses(foundCustomers);
           
           var insertAddresses = new List<Address>();
           foreach (var address in addresses)
           {

               insertAddresses.Add(address);
           }

           await new DbService().Insert(insertAddresses);
           
           await DownloadQuestionsAndOptionsAndSurvelemMaps(foundCustomers);

     
           var media = await new DataLoaderService().GetRichMedias(foundCustomers);

           var insertRichMedias = new List<RichMedia>();
           foreach (var option in media)
           {
               insertRichMedias.Add(option);
                
           }
           await new DbService().Insert(insertRichMedias);

           var page = 100;
           for (int i = 0; i < insertAddresses.Count; i = i + page)
           {
               var ads = insertAddresses.Skip(i).Take(page).ToList();
               await DownloadAddressQuestionGroupStatuses(ads);
               await DownloadSurvelems(foundCustomers, ads);

               Debug.WriteLine("Processed addresses: {0}/{1}",i,insertAddresses.Count);
               

           }




           
       }

       async Task  DownloadQuestionsAndOptionsAndSurvelemMaps(List<Customer> foundCustomers )
       {

           var survelemMaps = await new DataLoaderService().GetSurvelemMaps(foundCustomers);

           var insertSurvelemMaps = new List<SurvelemMap>();
           foreach (var option in survelemMaps)
           {
               insertSurvelemMaps.Add(option);
           }
           await new DbService().Insert(insertSurvelemMaps);

           ////

           var questions = await new DataLoaderService().GetQuestions(foundCustomers);

           var insertQuestions = new List<Question>();
           foreach (var question in questions)
           {
               insertQuestions.Add(question);
           }

           await new DbService().Insert(insertQuestions);

           ///

           var questionsCOunt = await new DataLoaderService().CountQuestions(foundCustomers);

        
           var dbCount = await new DbService().Count<Question>();

           if (questionsCOunt != dbCount)
           {
               throw new DataLoadException("Invalid count of Questions downloaded. Try again");
           }

           var options = await new DataLoaderService().GetOptions(foundCustomers);

           var insertOptions = new List<Option>();
           foreach (var option in options)
           {
               insertOptions.Add(option);
           }
           await new DbService().Insert(insertOptions);

           var optionsCount = await new DataLoaderService().CountOptions(foundCustomers);
           
           dbCount = await new DbService().Count<Option>();
           if (optionsCount != dbCount)
           {
               throw new DataLoadException("Invalid count of Options downloaded. Try again");
           }

       }

       async Task DownloadAddressQuestionGroupStatuses(List<Address> addresses)
       {
           var items = await new DataLoaderService().GetAddressQuestionGroupStatus(addresses.Select(x => x.Id).ToList());

           await new DbService().Save(items);
       }

       async Task DownloadSurvelems(List<Customer> customers, List<Address>  addresses, bool alwaysInsert = true)
       {
           
           foreach (var customer in customers)
           {
               var c = customer;
               foreach (var addressBatch in addresses.Batch(20))
               {

                   var addressesToQuery = addressBatch.Where(x => x.CustomerSurveyID == customer.CustomerSurveyID).ToList();

                   if (addressesToQuery.Count == 0) continue;

                   var tasks = addressesToQuery.Select(x => DownloadSurvelemsForAddress(c, x, alwaysInsert));


                   await Task.Factory.ContinueWhenAll(tasks.ToArray(), r =>
                   {

                   });

                   

                   
               }
           }


           //foreach (var customer in customers)
           //{

           //    foreach (var address in addresses)
           //    {
           //        if(address.CustomerSurveyID != customer.CustomerSurveyID) continue;
                   

           //        var survelems = await new DataLoaderService().GetSurvelems(customer,address);


           //         var insertSurvelems = new List<Survelem>();
           //        foreach (var s in survelems)
           //        {
           //            insertSurvelems.Add(s);
           //        }

           //        if (alwaysInsert)
           //        {
           //            await new DbService().Insert(insertSurvelems);
           //        }
           //        else
           //        {
           //            await new DbService().Save(insertSurvelems);
                   
           //        }
           //    }
           //}

          

          

       }



       async Task DownloadSurvelemsForAddress(Customer customer, Address address,bool alwaysInsert)
       {
           var survelems = await new DataLoaderService().GetSurvelems(customer, address);


           if (alwaysInsert)
           {
               await new DbService().Insert(survelems);
           }
           else
           {
               await new DbService().Save(survelems);

           }


         
       }

       public async Task RefreshAddressMoves(List<ServiceReference.AddressMove> moves)
       {
           if (moves == null || !moves.Any())
               return;

           foreach (var move in moves)
           {
               Helpers.DebugMessage(string.Format("Processing address move: {0}/{1}",moves.IndexOf(move),moves.Count));
               if (move.FromSurveyorId == StateService.CurrentUserId && move.ToSurveyorId == StateService.CurrentUserId)
               {
                   var foundCustomers = await new DataLoaderService().GetCustomers();
                   var addresses = await new DataLoaderService().GetAddresses(foundCustomers, move.AddressId);

                   var newAddress = addresses.FirstOrDefault();

                   if (newAddress != null)
                   {
                       await new DbService().Save(newAddress);
                   }
                   await new DataLoaderService().ProcessAddressMove(move.Id, true,true);
               }

               else if (move.FromSurveyorId == StateService.CurrentUserId && move.ToSurveyorId != StateService.CurrentUserId)
               {
                   var address = await new DbService().FindAddress(move.AddressId);
                   if (address != null)
                   {
                       address.RemoveDataAfterSync = true;

                       await new DbService().Save(address);

                       var survelems = await new DbService().GetSurvelemsByAddressUPRN(address.UPRN);
                       foreach (var s in survelems)
                       {
                           s.RemoveDataAfterSync = true;
                           await new DbService().Save(s);
                       }
                       
                   }
                   await new DataLoaderService().ProcessAddressMove(move.Id, true,false);

               }
               else if (move.ToSurveyorId == StateService.CurrentUserId && move.FromSurveyorId != StateService.CurrentUserId)
               {

                   var foundCustomers = await new DataLoaderService().GetCustomers();

                   var addresses = await new DataLoaderService().GetAddresses(foundCustomers,move.AddressId);

                   var newAddress = addresses.FirstOrDefault();

                   if (newAddress != null)
                   {
                       await new DbService().Save(newAddress);

                       var media = await new DataLoaderService().GetRichMedias(foundCustomers,newAddress.Id);

                       foreach (var m in media)
                       {
                           await new DbService().Save(m);
                       }


                       await DownloadSurvelems(foundCustomers, new List<Address> {newAddress},false);
                   }

                   await new DataLoaderService().ProcessAddressMove(move.Id, false,true);
               }

           }

       }

       public async Task RefreshUserData()
       {
           var foundCustomers = await new DataLoaderService().GetCustomers();

           foreach (var foundCustomer in foundCustomers)
           {
               await new DbService().Save(foundCustomer);
           }

           await new DbService().ClearTable<SurvelemMap>();
           await new DbService().ClearTable<Question>();
           await new DbService().ClearTable<Option>();

           await DownloadQuestionsAndOptionsAndSurvelemMaps(foundCustomers);
       }


       public async Task ClearAddress()
       {
           Helpers.ShowProgressIndicatorService("Clearing address");

           var address = StateService.CurrentAddress;
           address.IsCompleted = false;
           address.IsAlreadyCheckedPropertyType = false;

           await new DbService().Save(address, ESyncStatus.NotSynced);

           var status = await new DbService().FindAddressStatus(address.Id);

           if (status != null)
           {
               status.IsDeletedOnClient = true;
               await new DbService().Save(status, ESyncStatus.NotSynced);
           }

           var groups = await new DbService().GetAddressQuestionGroups(address.Id);

           foreach (var g in groups)
           {
               g.IsDeletedOnClient = true;
               await new DbService().Save(g, ESyncStatus.NotSynced);
           }


           var elems = await new DbService().GetSurvelemsByAddressUPRN(address.UPRN);



           foreach (var item in elems)
           {
               item.IsDeletedOnClient = true;
               await new DbService().Save(item, ESyncStatus.NotSynced);
           }

           var medias = await new DbService().GetRichMediasAddressUPRN(address.UPRN);

           foreach (var item in medias)
           {
               using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
               {
                   iso.DeleteFile(item.FileName);
               }

               item.IsDeletedOnClient = true;
               await new DbService().Save(item, ESyncStatus.NotSynced);
           }

          Helpers.HideProgressIndicatorService();
           
           Helpers.ShowMessageBox("All data cleared related to the address.");

           
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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using HuntersService.Contracts.Base;
using HuntersService.Entities;
using Netmera;
using Newtonsoft.Json;
using Omu.ValueInjecter;

namespace HuntersService.Contracts
{
    public class ImportRequest:BaseRequest
    {
        public bool ClearDb { get; set; }
        public bool CheckIfInserted { get; set; }
    }


    public class ImportSurveyorDataRequest : BaseRequest
    {
        public string NetmeraId { get; set; }
    }


    public class ImportSurveyorDataRequestHandler : RequestHandler<ImportSurveyorDataRequest, BaseReply>


     
    {

        List<Option> _options;

        protected override BaseReply Execute(ImportSurveyorDataRequest request)
        {

            NetmeraClient.init(ImportRequestHandler.KEY);

            _options = DbContext.Options.ToList();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {

                var reply = new BaseReply();


                if (string.IsNullOrEmpty(request.NetmeraId))
                {
                    var netmeraService = new NetmeraService("Surveyors");
                    netmeraService.setMax(100);

                    var surveyors = netmeraService.search();

                    foreach (var s in surveyors)
                    {
                        var o = JsonConvert.DeserializeObject<SurveyorOld>(s.data.ToString());


                        ImportSurveyoyr(o.id.ToString(),connection);
                    }
                }
                else
                {
                    ImportSurveyoyr(request.NetmeraId,connection);
                }

             


                return reply;
            }

        }

        void ImportSurveyoyr(string surveyorId, SqlConnection connection)
        {
            var addressService = new NetmeraService("Address_List1");

            addressService.whereEqual("Surveyor", surveyorId);

            var surveyor = DbContext.Surveyors.First(x => x.NetmeraId == surveyorId);


            SearchObjectsWithPaging<Address, AddressOld>(addressService, (db, s) =>
            {
                db.NetmeraId = s.AddressID;
                db.SurveyorId = surveyor.Id;
                db.TypeUpdated = s.PTUpdated;
                db.IsCompleted = s.Complete;
                db.UpdateDate = db.CreateDate;

                var status = new AddressStatus();
                status.IsCompleted = s.Complete;
                status.AddressId = db.Id;

                return false;

            });


            var downloadedAddresses = DbContext.Addresses.Where(x => x.SurveyorId == surveyor.Id).ToList();
            var page = 100;
            for (int i = 0; i < downloadedAddresses.Count; i = i + page)
            {
                var addresses = downloadedAddresses.Skip(i).Take(page);

                var service = new NetmeraService("Survelem");

                service.whereContainedIn("UPRN", addresses.Select(x => x.UPRN).ToList());

                ProcessSurvelems(service, connection);
            }
        }


        void ProcessSurvelems(NetmeraService service,SqlConnection connection)
        {


            SearchObjectsWithPaging<Survelem, SurvelemOld>(service,

           saveAction:  entities =>
           {
               var batches = entities.Batch(100);

               foreach (var batch in batches)
               {
                   foreach (var smallBatch in batch.Batch(10))
                   {

                       var q = string.Format("SELECT COUNT(Id) as Count FROM Survelems WHERE NetmeraId IN ({0})",
                           string.Join(",", smallBatch.Select(x => "'" + x.id + "'")).TrimEnd(','));

                       var c = connection.Query(q).ToList().First().Count;

                       if (c > 0 && c != smallBatch.Count())
                       {
                           foreach (var e in smallBatch)
                           {
                               if (CheckIfExists(connection, e))
                               {
                                   e.IsFoundInDb = true;
                               }
                           }
                           
                       }
                       else if (c == smallBatch.Count())
                       {
                           foreach (var e in smallBatch)
                           {
                               e.IsFoundInDb = true;
                           }
                       }

                   }
               }

                foreach (var e in entities)
                {
                    if (!e.IsFoundInDb)
                    {
                        Insert(e, connection);
                    }

                }
            });
        }


        bool CheckIfExists(SqlConnection connection,SurvelemOld e)
        {
            var r = connection.Query(
                "SELECT COUNT(ID) as Count FROM Survelems WHERE NetmeraId = @id", new {id = e.id}).ToList().First();

            return r.Count > 0;

        }

        void Insert(SurvelemOld e, SqlConnection connection)
        {
            var newId = Guid.NewGuid();

            Guid? optionId = null;
            Guid? optionId2 = null;

            if (!string.IsNullOrEmpty(e.OptionID))
                optionId = _options.First(x => x.NetmeraId == e.OptionID).Id;

            if (!string.IsNullOrEmpty(e.OptionID2ndry))
                optionId2 = _options.First(x => x.NetmeraId == e.OptionID2ndry).Id;

            connection.Execute(@"INSERT INTO [dbo].[Survelems]
           ([Id]
           ,[OptionID]
           ,[UPRN]
           ,[Question_Ref]
           ,[COMMENT]
           ,[Freetext]
           ,[BuildingType]
           ,[DateOfSurvey]
           ,[CustomerID]
           ,[CustomerSurveyID]
           ,[NetmeraId]
           ,[CreateDate]
           ,[OptionID2ndry]
            ,SqT1 
           ,SqT2
           ,SqT3 
           ,SqT4 
           ,SqT5 
           ,SqT6 
           ,SqT7 
           ,SqT8 
           ,SqT9 
           ,SqT10 
           ,SqT11
           ,SqT12 
           ,SqT13
           ,SqT14 
           ,SqT15 

           ,SqN1 
           ,SqN2
           ,SqN3 
           ,SqN4 
           ,SqN5 
           ,SqN6 
           ,SqN7 
           ,SqN8 
           ,SqN9 
           ,SqN10 
           ,SqN11
           ,SqN12 
           ,SqN13
           ,SqN14 
           ,SqN15 

)
     VALUES
           (@Id
           ,@OptionID
           ,@UPRN
           ,@Question_Ref
           ,@COMMENT
           ,@Freetext
           ,@BuildingType
           ,@DateOfSurvey
           ,@CustomerID
           ,@CustomerSurveyID
           ,@NetmeraId
           ,@CreateDate
           ,@OptionID2ndry

           ,@SqT1 
           ,@SqT2
           ,@SqT3 
           ,@SqT4 
           ,@SqT5 
           ,@SqT6 
           ,@SqT7 
           ,@SqT8 
           ,@SqT9 
           ,@SqT10 
           ,@SqT11
           ,@SqT12 
           ,@SqT13
           ,@SqT14 
           ,@SqT15 

           ,@SqN1 
           ,@SqN2
           ,@SqN3 
           ,@SqN4 
           ,@SqN5 
           ,@SqN6 
           ,@SqN7 
           ,@SqN8 
           ,@SqN9 
           ,@SqN10 
           ,@SqN11
           ,@SqN12 
           ,@SqN13
           ,@SqN14 
           ,@SqN15 


)", new
  {
      @Id = newId,
      OptionID = optionId,
      UPRN = e.UPRN,
      Question_Ref = e.Question_Ref,
      COMMENT = e.COMMENT,
      Freetext = e.Freetext,
      BuildingType = e.BuildingType,
      DateOfSurvey = e.DateOfSurvey,
      CustomerID = e.CustomerID,
      CustomerSurveyID = e.CustomerSurveyID,
      NetmeraId = e.id,
      CreateDate = DateTime.UtcNow,
      OptionID2ndry = optionId2,
      e.SqT1,
      e.SqT2,
      e.SqT3,
      e.SqT4,
      e.SqT5,
      e.SqT6,
      e.SqT7,
      e.SqT8,
      e.SqT9,
      e.SqT10,
      e.SqT11,
      e.SqT12,
      e.SqT13,
      e.SqT14,
      e.SqT15,

      e.SqN1,
      e.SqN2,
      e.SqN3,
      e.SqN4,
      e.SqN5,
      e.SqN6,
      e.SqN7,
      e.SqN8,
      e.SqN9,
      e.SqN10,
      e.SqN11,
      e.SqN12,
      e.SqN13,
      e.SqN14,
      e.SqN15,
  });

        }
    }

    public class ImportRequestHandler : RequestHandler<ImportRequest, BaseReply>
    {
        public static void ClearDatabase(DbContext context)
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var entities = objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName, DataSpace.CSpace).BaseEntitySets;
            var method = objectContext.GetType().GetMethods().First(x => x.Name == "CreateObjectSet");
            var objectSets = entities.Where(x => Type.GetType(x.ElementType.FullName) != null).Select(x => method.MakeGenericMethod(Type.GetType(x.ElementType.FullName))).Select(x => x.Invoke(objectContext, null));
            var tableNames = objectSets.Select(objectSet => (objectSet.GetType().GetProperty("EntitySet").GetValue(objectSet, null) as EntitySet).Name).ToList();

            foreach (var tableName in tableNames)
            {
                context.Database.ExecuteSqlCommand(string.Format("DELETE FROM {0}", tableName));
            }

            context.SaveChanges();
        }

        public const string KEY = "ZGoweEpuVTlOVEl6TWpjeU1HWmxOR0l3WWpjMFlUQm1NbVUwT1RsbUptRTlhSFZ1ZEdWeWMzZHBibkJvYjI1bEpn";

        private ImportRequest _request;
        protected override BaseReply Execute(ImportRequest request)
        {
            
            _request = request;
            _request.CheckIfInserted = true;
            var reply = new BaseReply();
            NetmeraClient.init(KEY);
                
            if(request.ClearDb)
                ClearDatabase(DbContext);

            Import();

            return reply;
        }

        void Import()
        {
            SearchObjectsWithPaging<SurveyType, SurveyTypesOld>(new NetmeraService("SurveyTypes"),
                (db, s) =>
                {
                    db.NetmeraId = s.ID;
                    return false;
                });


            SearchObjectsWithPaging<Customer, CustomerOld>(new NetmeraService("Customers"), (db, s) =>
            {
                db.NetmeraId = s.CustomerID.ToString();
                return false;
            });

          
            SearchObjectsWithPaging<SurvelemMap, SurvelemMapOld>(new NetmeraService("TblSurvelemMap"),
                (db, s) =>
                {
                    db.NetmeraId = s.SvmMapID;
                    return false;
                });

          

            SearchObjectsWithPaging<Question, QuestionOld>(new NetmeraService("Questions"), (db, s) =>
            {
                db.NetmeraId = s.Question_Ref;
                return false;
            });

         
            SearchObjectsWithPaging<Surveyor, SurveyorOld>(new NetmeraService("Surveyors"), (db, s) =>
            {
                db.NetmeraId = s.id.ToString();

                return false;
            });

            SearchObjectsWithPaging<Option, OptionOld>(new NetmeraService("Options"), (db, s) =>
            {
                db.NetmeraId = s.OptionId;
                db.OptionID = int.Parse(s.OptionId);
                db.Option_Number = s.Option_Number;

                return false;
            });


            SearchObjectsWithPaging<RichMedia,RichMediaOld>(new NetmeraService("RichMedia"), (db, s) =>
            {
                db.CloudPath = s.FileName;
                db.NetmeraId = s.ID;
                if (!string.IsNullOrEmpty(s.Option_ID))
                {
                    var option = DbContext.Options.FirstOrDefault(x => x.NetmeraId == s.Option_ID);

                    if (option != null)
                    {
                        db.Option_ID = option.Id;
                    }
                }

                return false;
            });

      




           // var addressService = new NetmeraService("Address_List1");
           // //REMOVE
           // addressService.whereContainedIn("Surveyor",new List<string>{"35"});

           // SearchObjectsWithPaging<Address, AddressOld>(addressService, (db, s) =>
           // {
           //     db.NetmeraId = s.AddressID;
           //     db.SurveyorId = DbContext.Surveyors.First(x=>x.NetmeraId == s.Surveyor).Id;
           //     db.TypeUpdated = s.PTUpdated;
           //     db.IsCompleted = s.Complete;

           //     var status = new AddressStatus();
           //     status.IsCompleted = s.Complete;
           //     status.AddressId = db.Id;

           //     return false;

           // });

            

           // //REMOVE
           // var downloadedAddresses = DbContext.Addresses.ToList();
           //var page = 100;
           // for (int i = 0; i < downloadedAddresses.Count; i = i + page)
           // {
           //     var addresses = downloadedAddresses.Skip(i).Take(page);

           //     var service = new NetmeraService("Survelem");

           //     service.whereContainedIn("UPRN", addresses.Select(x => x.UPRN).ToList());

           //     ProcessSurvelems(service);
           // }

            //REMOVE
            //ProcessSurvelems(new NetmeraService("Survelem"));

         



        }
        


      


    }

    public class CustomerOld
    {
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerShortName { get; set; }
        public string CustomerSurveyID { get; set; }
        public string SurveyImportDate { get; set; }
        public string Notes { get; set; }
        public bool Completed { get; set; }
        public string Surveyors { get; set; }

    }

    public class SurveyorOld
    {
        public long id { get; set; }
              
        public string first_name { get; set; }
              
        public string last_name { get; set; }
        [JsonProperty("Surveyor name")]
        public string SurveyorName { get; set; }
        public string Choose { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }


    public class AddressOld
    {
        public bool PTUpdated { get; set; }
        public string AddressID { get; set; }
        public string UPRN { get; set; }
        public string BlockUPRN { get; set; }
        public string FlatNo { get; set; }
        public string BuildingName { get; set; }
        public string StreetNo { get; set; }
        public string StreetName { get; set; }
        public string Postcode { get; set; }
        public string Type { get; set; }
        public string QuestionGrp { get; set; }
        public string Bedrooms { get; set; }
        public string Surveyor { get; set; }
        public string FullAddress { get; set; }
        public string Multipliers { get; set; }
        public string DateSurveyed { get; set; }
        public string SAPRating { get; set; }
        public string SAPBand { get; set; }
        public string LeaseHolderAddress { get; set; }
        public string Floor { get; set; }
        public bool CopyTo { get; set; }
        public bool Complete { get; set; }
        public string Visited { get; set; }
        public string Submit { get; set; }
        public string Submitted { get; set; }
        public bool AllowCopyFrom { get; set; }
        public string CopiedFrom { get; set; }
        [JsonProperty("Address line 1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("Address line 2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("Address line 3")]
        public string AddressLine3 { get; set; }

        [JsonProperty("Address line 4")]
        public string AddressLine4 { get; set; }

        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }

        public DateTime? UpdateDate { get; set; }



    }



    public class QuestionOld
    {
        public int Question_Order { get; set; }
        public string Question_Ref { get; set; }
        public string Main_Element { get; set; }
        public string Question_Heading { get; set; }
        public string Category { get; set; }
        public string Question_Type { get; set; }
        public string Apply_2nd_Question { get; set; }
        public string Unit { get; set; }
        public string SurveyType { get; set; }
        [JsonProperty("Decent Homes")]
        public string DecenWtHomes { get; set; }
        public bool ExcludeFromClone { get; set; }
        public string QuestionGroup { get; set; }
        public string Display { get; set; }
        public string LeaseholderQuestion { get; set; }
        public string AnswerRangeFrom { get; set; }
        public string AnswerRangeTo { get; set; }
        public string AnswerRangeTextF { get; set; }
        public string AnswerRangeTextT { get; set; }
        public string LookAtRange { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }
        public bool finishSection { get; set; }

    }

  
    public class OptionOld
    {
        public string OptionId { get; set; }
        public string Question_Ref { get; set; }
        public string Level { get; set; }
        public int Option_Number { get; set; }
        public string Text { get; set; }
        public string Jumpto { get; set; }
        public bool Choose { get; set; }
        public bool ConfirmationRequired { get; set; }
        public string LifeGuide { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }

        public bool Disable2nd { get; set; }

    }


    public class RichMediaOld 
    {
        public string ID { get; set; }
        public string UPRN { get; set; }
        public string Comments { get; set; }
        public string FileName { get; set; }
        public string Question_Ref { get; set; }
        public string Option_ID { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }
    }


    public class SurvelemOld
    {
        public string id { get; set; }
        public string OptionID { get; set; }
        public string OptionID2ndry { get; set; }
        public string UPRN { get; set; }
        public string Question_Ref { get; set; }
        public string COMMENT { get; set; }
        public string Freetext { get; set; }
        public string BuildingType { get; set; }
        public string DateOfSurvey { get; set; }
        public string SqT1 { get; set; }
        public string SqN1 { get; set; }
        public string SqT2 { get; set; }
        public string SqN2 { get; set; }
        public string SqT3 { get; set; }
        public string SqN3 { get; set; }
        public string SqT4 { get; set; }
        public string SqN4 { get; set; }
        public string SqT5 { get; set; }
        public string SqN5 { get; set; }
        public string SqT6 { get; set; }
        public string SqN6 { get; set; }
        public string SqT7 { get; set; }
        public string SqN7 { get; set; }
        public string SqT8 { get; set; }
        public string SqN8 { get; set; }

        public string SqT9 { get; set; }
        public string SqN9 { get; set; }
        public string SqT10 { get; set; }
        public string SqN10 { get; set; }
        public string SqT11 { get; set; }
        public string SqT12 { get; set; }
        public string SqT13 { get; set; }
        public string SqT14 { get; set; }
        public string SqT15 { get; set; }

        public string SqN11 { get; set; }
        public string SqN12 { get; set; }
        public string SqN13 { get; set; }
        public string SqN14 { get; set; }
        public string SqN15 { get; set; }

        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }


        public bool IsFoundInDb { get; set; }
    }


    public class SurveyTypesOld  
    {
        public string ID { get; set; }
        public string Name { get; set; }
        [JsonProperty("SurveyTypes")]
        public string Value { get; set; }
    }

    public class SurvelemMapOld {
        public string SvmMapID { get; set; }
        public string Question_Ref { get; set; }
        public string SqName { get; set; }
        public string Question_Heading { get; set; }
        public string Formats { get; set; }
        public string SurveyType { get; set; }
        public long CustomerID { get; set; }
        public string CustomerSurveyID { get; set; }
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
;

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
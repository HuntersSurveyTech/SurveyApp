using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netmera;
//using Newtonsoft.Json;
using SQLite;

namespace DataImporter
{
    class Program
    {

        private static int totalNum = 0;
        public const string KEY = "ZGoweEpuVTlOVEl6TWpjeU1HWmxOR0l3WWpjMFlUQm1NbVUwT1RsbUptRTlhSFZ1ZEdWeWMzZHBibkJvYjI1bEpn";
        static void Main(string[] args)
        {
            NetmeraClient.init(KEY);

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sqlite3");

            foreach (var f in files)
            {
                Console.WriteLine("Processing: " + f);

                var c = new SQLiteConnection(f);

                var survelems = c.Table<Survelem>().ToList();
                
                ProcessSurvelemes(survelems);

                c.Close();
                c.Dispose();

                File.Move(f,f.Replace("sqlite3","sqlite3+"));
                Console.WriteLine("Inserted " + totalNum +"records");
            }
            Console.WriteLine("Done");
            Console.ReadLine();


        }

        static void ProcessSurvelemes(List<Survelem> survelems)
        {
            var requestCount = 10;
            
            for (int i = 0; i <= survelems.Count / requestCount; i++)
            {
                var items = survelems.Skip(i * requestCount).Take(requestCount).ToList();

                var conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; " + "Data Source=C:\\NetmeraRMTools\\sqliteInsertLog\\checkedLog.accdb");
                //conn.Open();

                foreach (var survelem in items)
                {
                    //Thread.Sleep(5);
                    NetmeraContent e;
                    var inNetmera = 1;

                    conn.Open();
                    var cmdSelect = new OleDbCommand("Select * FROM Log WHERE localID ='" + survelem.id + "'", conn);
                    var reader  = cmdSelect.ExecuteReader();

                    if (!(reader.HasRows))
                    {
                        e = FindNetmeraContent("Survelem", "id", survelem.id);
                        var cmdInsert = new OleDbCommand("INSERT INTO Log (localID) VALUES (@localID)", conn);
                        cmdInsert.Parameters.AddWithValue("@localID", survelem.id);
                        cmdInsert.ExecuteNonQuery();
                        if (e == null)
                        {
                            inNetmera = 0;
                        }
                    }
                    conn.Close();

                    if (inNetmera == 0)
                    {
                        e = new NetmeraContent("Survelem");
                    }
                    else
                    {
                        //conn.Close();
                        continue;
                    }

                    if (e != null)
                    {
                        e.add("COMMENT", survelem.COMMENT);
                        e.add("CustomerID", survelem.CustomerID);
                        e.add("CustomerSurveyID", survelem.CustomerSurveyID);
                        e.add("DateOfSurvey", survelem.DateOfSurvey);
                        e.add("OptionID", survelem.OptionID ?? "");
                        e.add("OptionID2ndry", survelem.OptionID2ndry ?? "");
                        e.add("Question_Ref", survelem.Question_Ref);
                        e.add("UPRN", survelem.UPRN);
                        e.add("id", survelem.id);
                        e.add("Freetext", survelem.Freetext ?? "");
                        e.add("BuildingType", survelem.BuildingType ?? "");


                        e.add("SqN1", survelem.SqN1 ?? "");
                        e.add("SqN10", survelem.SqN10 ?? "");
                        e.add("SqN11", survelem.SqN11 ?? "");
                        e.add("SqN12", survelem.SqN12 ?? "");
                        e.add("SqN13", survelem.SqN13 ?? "");
                        e.add("SqN14", survelem.SqN14 ?? "");
                        e.add("SqN15", survelem.SqN15 ?? "");
                        e.add("SqN2", survelem.SqN2 ?? "");
                        e.add("SqN3", survelem.SqN3 ?? "");
                        e.add("SqN4", survelem.SqN4 ?? "");
                        e.add("SqN5", survelem.SqN5 ?? "");
                        e.add("SqN6", survelem.SqN6 ?? "");
                        e.add("SqN7", survelem.SqN7 ?? "");
                        e.add("SqN8", survelem.SqN8 ?? "");
                        e.add("SqN9", survelem.SqN9 ?? "");
                        e.add("SqT1", survelem.SqT1 ?? "");
                        e.add("SqT10", survelem.SqT10 ?? "");
                        e.add("SqT11", survelem.SqT11 ?? "");
                        e.add("SqT12", survelem.SqT12 ?? "");
                        e.add("SqT13", survelem.SqT13 ?? "");
                        e.add("SqT14", survelem.SqT14 ?? "");
                        e.add("SqT15", survelem.SqT15 ?? "");
                        e.add("SqT2", survelem.SqT2 ?? "");
                        e.add("SqT3", survelem.SqT3 ?? "");
                        e.add("SqT4", survelem.SqT4 ?? "");
                        e.add("SqT5", survelem.SqT5 ?? "");
                        e.add("SqT6", survelem.SqT6 ?? "");
                        e.add("SqT7", survelem.SqT7 ?? "");
                        e.add("SqT8", survelem.SqT8 ?? "");
                        e.add("SqT9", survelem.SqT9 ?? "");

                    }

                    var s = survelem;


                    ProcessItem(s, e);

                }
            }
        }

        static async void ProcessItem(Survelem survelem, NetmeraContent e)
        {
            Create<Survelem>(e, survelem.id);
        }


        static void Create<T>(NetmeraContent content, string id)
        {



            try
            {
                //if ((content.data.ToString().Contains("&")))
                //{
                //    Dictionary<string, string> addressDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(content.data.ToString());
                //    foreach (KeyValuePair<string, string> keyValuePair in addressDictionary)
                //    {
                //        if (keyValuePair.Value.Contains("&"))
                //            content.add(keyValuePair.Key, keyValuePair.Value.Replace("&", "%26"));
                //    }
                //}
                content.create();
                var conn2 = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; " + "Data Source=C:\\NetmeraRMTools\\sqliteInsertLog\\insertlog.accdb");
                var cmdInsert = new OleDbCommand("INSERT INTO Log (UPRN,Question_Ref,localID,netmeraPath) VALUES (@UPRN,@Question_Ref,@localID,@netmeraPath)", conn2);
                cmdInsert.Parameters.AddWithValue("@UPRN", content.getString("UPRN"));
                cmdInsert.Parameters.AddWithValue("@Question_Ref", content.getString("Question_Ref"));
                cmdInsert.Parameters.AddWithValue("@localID", content.getString("id"));
                cmdInsert.Parameters.AddWithValue("@netmeraPath", content.getPath());

                conn2.Open();
                cmdInsert.ExecuteNonQuery();
                conn2.Close();
                totalNum = totalNum + 1;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Create error: " + ex.Message);
            }



        }


        static NetmeraContent FindNetmeraContent(string table, string idField, object idValue)
        {

            NetmeraService service = new NetmeraService(table);
            service.whereEqual(idField, idValue);

            var items = SearchNetmeraContent(service);

            if (items.IsSuccess)
            {
                return items.Data;
            }

            return null;
        }



        static ApiResponse<NetmeraContent> SearchNetmeraContent(NetmeraService service)
        {
            service.setMax(1);

            try
            {
                var list = service.search();

                if (list.Any())
                {
                    return new ApiResponse<NetmeraContent>() { Data = list[0], IsSuccess = true };
                }

                return new ApiResponse<NetmeraContent>() { IsSuccess = false, Message = "Object was not found" };
            }
            catch (Exception tpsEx)
            {
                Console.WriteLine("Search error: " + tpsEx.Message);
                return new ApiResponse<NetmeraContent>() { IsSuccess = false, Message = "Object was not found" };
            }
            

        }

        public class ApiResponse<T>
        {
            public Exception Exception { get; set; }


            public T Data { get; set; }

            public bool IsSuccess { get; set; }
            public string Message { get; set; }
        }

    }

    public class Survelem : Entity
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
        public string SpotPriceFF { get; set; }
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
    }

    public class Id
    {
        public int inc { get; set; }
        public int machine { get; set; }
        public bool @new { get; set; }
        public long time { get; set; }
        public int timeSecond { get; set; }
    }

    public class Entity
    {
        public Entity()
        {

        }

        public string parentPath { get; set; }

        public string contentPrivacy { get; set; }

        [Ignore]
        public Id _id { get; set; }

        public byte SyncStatus { get; set; }
        public string SyncErrorId { get; set; }

        [PrimaryKey]
        public string Identity { get; set; }

        public bool IsCreatedOnClient { get; set; }


    }
}

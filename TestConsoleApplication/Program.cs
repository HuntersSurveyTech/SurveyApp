using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TestConsoleApplication.ServiceReference1;

namespace TestConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
        //    var client = new ServiceReference1.HuntersServiceClient();

        // //var login = client.ProcessRequest(new LoginRequest() { Login = "test1", Password = "7110eda4d09e062aa5e4a390b0a572ac0d2c0220" }) as LoginReply;

        //// var customers = client.ProcessRequest(new GetSurvelemsRequest() { Customer = "6aa1fe09-60c8-4bdf-98c8-59e022f29eb8", UPRN = "1019110", UserAuthId = login.UserId.Value,ItemsPerPage = 100}) as GetSurvelemsReply;


        //   //var r = client.ProcessRequest(new GetSurvelemsRequest() { Customers = new List<string>() 
        //   //{ "6aa1fe09-60c8-4bdf-98c8-59e022f29eb8" },
        //   //                                                  ItemsPerPage = 10,
        //   //                                                  UPRNs = new List<string>() { "-dc84e024-ea23-4bf2-b9e7-d72a09f97eee" },
        //   //                                                  UserAuthId = login.UserId.Value
        //   //});
        //    //var questions =
        //    //    client.ProcessRequest(new GetQuestionsRequest()
        //    //    {
        //    //        Customers = new List<string>() { "6aa1fe09-60c8-4bdf-98c8-59e022f29eb8" },
        //    //        Offset = 650,
        //    //        ItemsPerPage = 50,
        //    //        UserAuthId = login.UserId.Value
        //    //    });

        //  var reply = client.ProcessRequest(new ImportSurveyorDataRequest(){NetmeraId = ""});
        //  // var reply = client.ProcessRequest(new ImportRequest() {  });

        //    //Console.WriteLine(reply);
        //    Console.WriteLine("IsSuccess = {0},Message={1}",reply.IsSuccess,reply.Data);



        //    Console.WriteLine("Done");


            var bytes = ConvertToPDF(File.ReadAllBytes("7.jpg"));

            File.WriteAllBytes("test.pdf",bytes);

            Console.ReadLine();
        }

        public static byte[] ConvertToPDF(byte[] data)
        {
            Document document = new Document();
            using (var imageStream = new MemoryStream(data))
            {

                using (var pdfStream = new MemoryStream())
                {
                    PdfWriter.GetInstance(document, pdfStream);
                    document.Open();

                    var image = iTextSharp.text.Image.GetInstance(imageStream);
                    image.ScaleToFit(document.PageSize.Width - 1,document.PageSize.Height);
                    image.SetAbsolutePosition(0, document.PageSize.Height - image.ScaledHeight);
                    document.Add(image);
   
                 document.Close();
                   


                   // pdfStream.Seek(0, SeekOrigin.Begin);

                    return pdfStream.ToArray();
                }
            }

        }
    }
}

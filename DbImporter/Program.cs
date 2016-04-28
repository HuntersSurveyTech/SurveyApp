using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.Services;
using ServiceReference=HuntersWP.ServiceReference;


namespace DbImporter
{
    public class HuntersWP
    {
        static void Main(string[] args)
        {
            StateService.CurrentUserId = new Guid("cebd5b5e-4315-4798-a026-1d5b52653ccf");

            StateService.CurrentServiceUri = "http://huntersdatamigrate.azurewebsites.net";

            ///

            AutoMapper.Mapper.CreateMap<SurvelemMap, ServiceReference.SurvelemMap>();
            AutoMapper.Mapper.CreateMap<ServiceReference.SurvelemMap, SurvelemMap>();
            
            AutoMapper.Mapper.CreateMap<Question, ServiceReference.Question>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Question, Question>();

            AutoMapper.Mapper.CreateMap<Option, ServiceReference.Option>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Option, Option>();
      
            AutoMapper.Mapper.CreateMap<RichMedia, ServiceReference.RichMedia>();
            AutoMapper.Mapper.CreateMap<ServiceReference.RichMedia, RichMedia>();

            AutoMapper.Mapper.CreateMap<SurveyType, ServiceReference.SurveyType>();
            AutoMapper.Mapper.CreateMap<ServiceReference.SurveyType, SurveyType>();

            AutoMapper.Mapper.CreateMap<Customer, ServiceReference.Customer>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Customer, Customer>();

            AutoMapper.Mapper.CreateMap<Address, ServiceReference.Address>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Address, Address>();
            
            AutoMapper.Mapper.CreateMap<Survelem, ServiceReference.Survelem>();
            AutoMapper.Mapper.CreateMap<ServiceReference.Survelem, Survelem>();


            AutoMapper.Mapper.CreateMap<AddressStatus, ServiceReference.AddressStatus>();
            AutoMapper.Mapper.CreateMap<ServiceReference.AddressStatus, AddressStatus>();

            
            //AutoMapper.Mapper.CreateMap<SurvelemSecond, ServiceReference.SurvelemSecond>();
            //AutoMapper.Mapper.CreateMap<ServiceReference.SurvelemSecond, SurvelemSecond>();

            
            AutoMapper.Mapper.CreateMap<QAAddressComment, ServiceReference.QAAddressComment>();
            AutoMapper.Mapper.CreateMap<ServiceReference.QAAddressComment, QAAddressComment>();


            AutoMapper.Mapper.CreateMap<QAAddress, ServiceReference.QAAddress>();
            AutoMapper.Mapper.CreateMap<ServiceReference.QAAddress, QAAddress>();


            AutoMapper.Mapper.CreateMap<AddressQuestionGroupStatus, ServiceReference.AddressQuestionGroupStatus>();
            AutoMapper.Mapper.CreateMap<ServiceReference.AddressQuestionGroupStatus, AddressQuestionGroupStatus>();

   
            DbService.DB_PATH = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
          

            var dbs = Directory.GetFiles(DbService.DB_PATH, "*.sqlite");

            if (dbs.Any())
            {
                DbService.DB_NAME = Path.GetFileName(dbs[0]);
            }
            else
            {
                Console.WriteLine("Not found db");
                Console.ReadLine();
                return;
            }

            StartSync();

            Console.ReadLine();

        }

        async static void StartSync()
        {
            try
            {
                await InternalSyncEngine.Execute(false);
                Console.WriteLine("Sync done");
            }
            catch (Exception ex)
            {
               Console.WriteLine("Sync error:" + ex.Message);
            }

            
        }
    }
}

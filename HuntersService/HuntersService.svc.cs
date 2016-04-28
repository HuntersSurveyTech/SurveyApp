using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using HuntersService.Contracts;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

using Ninject;

namespace HuntersService
{

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class HuntersService : IHuntersService
    {
        private static bool Initialized;
        private static readonly object Locker = new object();
        private static StandardKernel Kernel;
        public HuntersService()
		{
			if (!Initialized)
			{
				lock (Locker)
				{
					if (!Initialized)
					{
                        //CloudStorageAccount.SetConfigurationSettingPublisher(
                        //    (configName, configSetter) => configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)));

						//InitializeStorages();
                        InitializeDb();
						InitializeIoCContainer();

						Initialized = true;
					}
				}
			}
		}

        public BaseReply ProcessRequest(BaseRequest request)
        {
            //Debug.WriteLine("Process Request started");

            var handler = Kernel.Get<IRequestHandler>(request.GetType().Name);
            using (var db = new MyDbContext())
            {
                var context = new RequestContext(db);
                var result = handler.Execute(request, context);

                return result;
            }
        }

        static void InitializeDb()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MyDbContext, MyConfiguration>());
        }

        private static void InitializeIoCContainer()
        {
            var kernel = new StandardKernel();
            kernel.Bind<MyDbContext>().ToSelf();
            Register<LoginRequestHandler>(kernel);
            Register<GetCustomersRequestHandler>(kernel);
            Register<GetSurveyTypesRequestHandler>(kernel);
            Register<GetAddressesRequestHandler>(kernel);
            Register<GetQuestionsRequestHandler>(kernel);
            Register<GetOptionsRequestHandler>(kernel);
            Register<GetRichMediasRequestHandler>(kernel);
            Register<GetSurvelemMapsRequestHandler>(kernel);
            Register<GetSurvelemsRequestHandler>(kernel);
            Register<CheckItemsRequestHandler>(kernel);

            Register<SaveAddressRequestHandler>(kernel);
            Register<SaveRichMediasRequestHandler>(kernel);
            Register<SaveSurvelemRequestHandler>(kernel);
            Register<SaveAddressStatusRequestHandler>(kernel);
            Register<ProcessAddressMoveRequestHandler>(kernel);
            Register<SaveQAAddressRequestHandler>(kernel);
            Register<SaveQAAddressCommentRequestHandler>(kernel);
            Register<SaveAddressQuestionGroupStatusRequestHandler>(kernel);

            Register<GetAddressQuestionGroupStatusRequestHandler>(kernel);

            Register<ImportRequestHandler>(kernel);

            Register<ImportSurveyorDataRequestHandler>(kernel);


            Register<UploadFileRequestHandler>(kernel);

            Kernel = kernel;
        }

        private static void Register<THandler>(StandardKernel kernel)
            where THandler : IRequestHandler
        {
            kernel
                .Bind<IRequestHandler>()
                .To<THandler>()
                .Named(typeof(THandler).BaseType.GetGenericArguments()[0].Name);
        }


      
    }

    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using HuntersWP.Models;
using HuntersWP.ServiceReference;
using Address = HuntersWP.ServiceReference.Address;
using RichMedia = HuntersWP.ServiceReference.RichMedia;
using Survelem = HuntersWP.ServiceReference.Survelem;
using AddressStatus = HuntersWP.ServiceReference.AddressStatus;
using QAAddress = HuntersWP.ServiceReference.QAAddress;
using QAAddressComment = HuntersWP.ServiceReference.QAAddressComment;
using AddressQuestionGroupStatus = HuntersWP.ServiceReference.AddressQuestionGroupStatus;

namespace HuntersWP.Services
{
    public class MyServiceClient
    {
        public static int LOAD_ITEMS_PER_PAGE = 500;
        public static int SAVE_ITEMS_PER_PAGE = 100;


        public async Task<LoginReply> Login(string login, string password)
        {
            return await ProcessRequest<LoginReply>(new LoginRequest(){Login = login,Password = password});
        }

        public async Task<BaseReply> UploadFile(byte[] image,string fileName, bool isCreatePDF,
            string PDFFileName, string watermarkText)
        {
            return await ProcessRequest<BaseReply>(new UploadFileRequest()
            {
                File = image,
                FileName = fileName,
                IsCreatePDF = isCreatePDF,
                PDFFileName = PDFFileName,
                WatermarkText = watermarkText
            });
        }

        public async Task<BaseReply> ProcessAddressMove(Guid id, bool isFrom,bool isTo)
        {
            return await ProcessRequest<BaseReply>(new ProcessAddressMoveRequest() {Id =  id,IsFrom = isFrom,IsTo = isTo});
        }

        public async Task<BaseReply> SaveAddressQuestionGroupStatus(List<AddressQuestionGroupStatus> items)
        {
            return await ProcessRequest<BaseReply>(new SaveAddressQuestionGroupStatusRequest() { Items = items });
        }


        public async Task<BaseReply> SaveAddressStatuses(List<AddressStatus> items)
        {
            return await ProcessRequest<BaseReply>(new SaveAddressStatusRequest() { Items = items });
        }

        public async Task<BaseReply> SaveAddresses(List<Address> items)
        {
            return await ProcessRequest<BaseReply>(new SaveAddressRequest() { Items = items });
        }

        public async Task<BaseReply> SaveQAAddresses(List<QAAddress> items)
        {
            return await ProcessRequest<BaseReply>(new SaveQAAddressRequest() { Items = items });
        }

        public async Task<BaseReply> SaveQAAddressComments(List<QAAddressComment> items)
        {
            return await ProcessRequest<BaseReply>(new SaveQAAddressCommentsRequest() { Items = items });
        }

        public async Task<BaseReply> SaveRichMedias(List<RichMedia> items)
        {
            return await ProcessRequest<BaseReply>(new SaveRichMediaRequest() { Items = items });
        }

        public async Task<BaseReply> SaveSurvelems(List<Survelem> items)
        {
            return await ProcessRequest<BaseReply>(new SaveSurvelemRequest() { Items = items });
        }


        public async Task<GetCustomersReply> GetCustomers()
        {
            var reply = await ProcessRequest<GetCustomersReply>(new GetCustomersRequest());

            return reply;
        }

        public async Task<GetAddressesReply> GetAddresses(int offset, List<string> customers, Guid? id = null)
        {
            var reply =
                await
                    ProcessRequest<GetAddressesReply>(new GetAddressesRequest()
                    {
                        ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                        Offset = offset,
                        Customers = customers,
                        Id = id
                    });
            return reply;

        }

        public async Task<GetAddressQuestionGroupStatusReply> GetAddressQuestionGroupStatus(List<Guid> adressIds)
        {
            var reply =
                await
                    ProcessRequest<GetAddressQuestionGroupStatusReply>(new GetAddressQuestionGroupStatusRequest()
                    {
                        AddressIds = adressIds,
                    });
            return reply;

        }



        public async Task<GetSurvelemsReply> GetSurvelems(int offset, string customer, string uprn)
        {
            var reply =
                await
                    ProcessRequest<GetSurvelemsReply>(new GetSurvelemsRequest()
                    {
                        ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                        Offset = offset,
                        Customer = customer,
                        UPRN = uprn
                    });
            return reply;

        }


        public async Task<GetOptionsReply> GetOptions(int offset,List<string> customers)
        {
            var reply =
                await
                    ProcessRequest<GetOptionsReply>(new GetOptionsRequest()
                    {
                        ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                        Offset = offset,
                        Customers = customers
                    });
            return reply;

        }

        public async Task<GetQuestionsReply> GetQuestions(int offset, List<string> customers)
        {
            var reply =
                await
                    ProcessRequest<GetQuestionsReply>(new GetQuestionsRequest(){
                        ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                        Offset = offset,
                        Customers = customers
                    });
            return reply;

        }

        public async Task<GetQuestionsReply> CountQuestions(List<string> customers)
        {
            var reply =
                await
                    ProcessRequest<GetQuestionsReply>(new GetQuestionsRequest()
                    {
                        ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                        Offset = 0,
                        Customers = customers
                    });
            return reply;

        }

        public async Task<GetOptionsReply> CountOptions(List<string> customers)
        {
            var reply =
                await
                    ProcessRequest<GetOptionsReply>(new GetOptionsRequest()
                    {
                        ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                        Offset = 0,
                        Customers = customers
                    });
            return reply;

        }


        public async Task<GetSurvelemMapsReply> GetSurvelemMaps(int offset, List<string> customers)
        {
            var reply =
              await
                  ProcessRequest<GetSurvelemMapsReply>(new GetSurvelemMapsRequest()
                  {
                      ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                      Offset = offset,
                      Customers = customers
                  });
            return reply;
        }

        public async Task<GetRichMediasReply> GetRichMedias(int offset, List<string> customers, Guid? addressId = null)
        {
            var reply =
              await
                  ProcessRequest<GetRichMediasReply>(new GetRichMediasRequest()
                  {
                      ItemsPerPage = LOAD_ITEMS_PER_PAGE,
                      Offset = offset,
                      Customers = customers,
                      AddressId = addressId
                  });
            return reply;
        }

        public async Task<GetSurveyTypesReply> GetSurveyTypes()
        {
            var reply = await ProcessRequest<GetSurveyTypesReply>(new GetSurveyTypesRequest());

            return reply;
        }

        

        async Task<T> ProcessRequest<T>(BaseRequest req) where T:BaseReply,new()
        {
            TaskCompletionSource<T> _reply = new TaskCompletionSource<T>();

            if (req is BaseAuthRequest)
            {
                (req as BaseAuthRequest).UserAuthId = StateService.CurrentUserId;
            }

            var client = new HuntersServiceClient(new BasicHttpBinding()
            {
                MaxReceivedMessageSize = 2147483647,
                OpenTimeout = TimeSpan.FromSeconds(30),
                CloseTimeout = TimeSpan.FromSeconds(30),
                ReceiveTimeout = TimeSpan.FromSeconds(300),
                SendTimeout = TimeSpan.FromSeconds(300)
            },
                new EndpointAddress(string.Format("{0}/HuntersService.svc", StateService.CurrentServiceUri)));

            client.ProcessRequestCompleted += delegate(object sender, ProcessRequestCompletedEventArgs args)
            {
                if (args.Error == null)
                {
                    var reply = args.Result;

                    _reply.SetResult(reply as T);
                }
                else
                {
                    _reply.SetResult(new T(){IsSuccess = false,Data = args.Error.Message});
                }

            };

            client.ProcessRequestAsync(req);


            return await _reply.Task;
        }



    }
}

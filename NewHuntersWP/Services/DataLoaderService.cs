using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HuntersWP.Db;
using HuntersWP.Models;
using HuntersWP.ServiceReference;
using Question = HuntersWP.Models.Question;
using Address = HuntersWP.Models.Address;
using AddressStatus = HuntersWP.Models.AddressStatus;
using Customer = HuntersWP.Models.Customer;
using Option = HuntersWP.Models.Option;
using SurveyType = HuntersWP.Models.SurveyType;
using SurvelemMap = HuntersWP.Models.SurvelemMap;
using RichMedia = HuntersWP.Models.RichMedia;
using Survelem = HuntersWP.Models.Survelem;
using Entity = HuntersWP.Models.Entity;
using QAAddress = HuntersWP.Models.QAAddress;
using QAAddressComment = HuntersWP.Models.QAAddressComment;
using AddressQuestionGroupStatus = HuntersWP.Models.AddressQuestionGroupStatus;
namespace HuntersWP.Services
{
    public class UploadFileResult
    {
        public bool IsSuccess { get; set; }

        public string Data { get; set; }
    }

    public class DataLoaderService
    {
        public async Task<UploadFileResult> UploadFile(byte[] image, string fileName, bool isCreatePDF,
            string PDFFileName, string watermarkText)
        {
            var reply = await new MyServiceClient().UploadFile(image,fileName,isCreatePDF,PDFFileName,watermarkText);

            if (!reply.IsSuccess)
            {
                Helpers.LogEvent("UploadFileFailed",new Dictionary<string, string>()
                {
                    {"Error",reply.Data},
                     {"File",fileName}
                });
                return new UploadFileResult() {Data = reply.Data};
                //throw new DataLoadException(reply.Data);
            }

            return new UploadFileResult() { IsSuccess = true, Data = reply.Data };
        }

        public async Task ProcessAddressMove(Guid id, bool isFrom, bool isToo)
        {
            var reply = await new MyServiceClient().ProcessAddressMove(id, isFrom, isToo);

            if (!reply.IsSuccess)
            {
                throw new DataLoadException(reply.Data);
            }
        }

        public async Task SaveQAAddresses(List<QAAddress> items)
        {
            await SaveItems<QAAddress, ServiceReference.QAAddress>(items, (i) =>
            {
                return new MyServiceClient().SaveQAAddresses(i);
            });

        }

        public async Task SaveQAAddressComments(List<QAAddressComment> items)
        {
            await SaveItems<QAAddressComment, ServiceReference.QAAddressComment>(items, (i) =>
            {
                return new MyServiceClient().SaveQAAddressComments(i);
            });

        }



        public async Task<List<DataLoadResult>> SaveAddresses(List<Address> items)
        {
            return await SaveItems<Address, ServiceReference.Address>(items, (i) =>
            {
                return new MyServiceClient().SaveAddresses(i);
            });

        }


        public async Task SaveAddressStatuses(List<AddressStatus> items)
        {
            await SaveItems<AddressStatus, ServiceReference.AddressStatus>(items, (i) =>
            {
                return new MyServiceClient().SaveAddressStatuses(i);
            });

        }

        public async Task SaveAddressQuestionGroupStatus(List<AddressQuestionGroupStatus> items)
        {
            await SaveItems<AddressQuestionGroupStatus, ServiceReference.AddressQuestionGroupStatus>(items, (i) =>
            {
                return new MyServiceClient().SaveAddressQuestionGroupStatus(i);
            });

        }

        public async Task SaveRichMedias(List<RichMedia> items)
        {
            await SaveItems<RichMedia, ServiceReference.RichMedia>(items, (i) =>
            {
                return new MyServiceClient().SaveRichMedias(i);
            });
        }

        public async Task<List<DataLoadResult>> SaveSurvelems(List<Survelem> items)
        {
            return await SaveItems<Survelem, ServiceReference.Survelem>(items, (i) =>
            {
                return new MyServiceClient().SaveSurvelems(i);
            });
        }

        public async Task<List<Customer>> GetCustomers()
        {
            var reply = await new MyServiceClient().GetCustomers();
            if (reply.IsSuccess)
            {
                return Convert<Customer, HuntersWP.ServiceReference.Customer>(reply.Items);
            }

            throw new DataLoadException();
        }

        public async Task<List<SurveyType>> GetSurveyTypes()
        {
            var reply = await new MyServiceClient().GetSurveyTypes();
            
            if (reply.IsSuccess)
            {
                return Convert<SurveyType,HuntersWP.ServiceReference.SurveyType>(reply.Items);
            }

            throw new DataLoadException();
        }



        public async Task<List<AddressQuestionGroupStatus>> GetAddressQuestionGroupStatus(List<Guid> addressIds)
        {
            var reply = await new MyServiceClient().GetAddressQuestionGroupStatus(addressIds);

            if (reply.IsSuccess)
            {
                return Convert<AddressQuestionGroupStatus, HuntersWP.ServiceReference.AddressQuestionGroupStatus>(reply.Items);
            }

            throw new DataLoadException();
        }


        public async Task<List<SurvelemMap>> GetSurvelemMaps(List<Customer> customers)
        {
            return await LoadWithPaging<SurvelemMap, GetSurvelemMapsReply, HuntersWP.ServiceReference.SurvelemMap>((i) =>
            {
                return new MyServiceClient().GetSurvelemMaps(i,customers.Select(x=>x.CustomerSurveyID).ToList());
            },
            r => r.Items);
        }

        public async Task<List<Survelem>> GetSurvelems(Customer customer, Address address)
        {
            return await LoadWithPaging<Survelem, GetSurvelemsReply, HuntersWP.ServiceReference.Survelem>((i) =>
            {
                return new MyServiceClient().GetSurvelems(i, customer.CustomerSurveyID,address.UPRN);
            },
            r => r.Items);
        }

        public async Task<int> CountQuestions(List<Customer> customers)
        {
            var r = await new MyServiceClient().CountQuestions(customers.Select(x => x.CustomerSurveyID).ToList());

            if (r.IsSuccess) return r.TotalCount;

            throw new DataLoadException();
        }


        public async Task<int> CountOptions(List<Customer> customers)
        {
            var r = await new MyServiceClient().CountOptions(customers.Select(x => x.CustomerSurveyID).ToList());

            if (r.IsSuccess) return r.TotalCount;

            throw new DataLoadException();
        }


        public async Task<List<RichMedia>> GetRichMedias(List<Customer> customers, Guid? addressId = null)
        {
            return await LoadWithPaging<RichMedia, GetRichMediasReply, HuntersWP.ServiceReference.RichMedia>((i) =>
            {
                return new MyServiceClient().GetRichMedias(i, customers.Select(x => x.CustomerSurveyID).ToList(), addressId);
            },
            r => r.Items);
        }


        public async Task<List<Address>> GetAddresses(List<Customer> customers,Guid? id = null)
        {
            return await LoadWithPaging<Address, GetAddressesReply, HuntersWP.ServiceReference.Address>((i) =>
            {
                return new MyServiceClient().GetAddresses(i, customers.Select(x => x.CustomerSurveyID).ToList(),id);
            },
            r=>r.Items);
        }


        public async Task<List<Option>> GetOptions(List<Customer> customers)
        {
            return await LoadWithPaging<Option, GetOptionsReply, HuntersWP.ServiceReference.Option>((i) =>
            {
                return new MyServiceClient().GetOptions(i, customers.Select(x => x.CustomerSurveyID).ToList());
            },
            r => r.Items);
        }

        public async Task<List<Question>> GetQuestions(List<Customer> customers)
        {
            return await LoadWithPaging<Question, GetQuestionsReply, HuntersWP.ServiceReference.Question>((i) =>
            {
                return new MyServiceClient().GetQuestions(i, customers.Select(x => x.CustomerSurveyID).ToList());
            },
            r => r.Items);
        }

        public async Task<List<DataLoadResult>> SaveItems<T, V>(List<T> items, 
            Func<List<V>, 
            Task<BaseReply>> loader)
            where T : Entity, new()
            where V : HuntersWP.ServiceReference.Entity
        {
            var result = new List<DataLoadResult>();

            if (!items.Any()) return result;

            //foreach (var item in items)
            //{
            //    await new DbService().Save(item, ESyncStatus.InProcess, null, true);
            //}

            var serviceItems = Convert<V, T>(items);
            int counter = 0;
            foreach (var batch in serviceItems.Batch(MyServiceClient.SAVE_ITEMS_PER_PAGE))
            {
                var batchList = batch.ToList();

                var reply = await loader(batchList);

                if (!reply.IsSuccess)
                {
                    foreach (var v in batchList)
                    {
                        await new DbService().Save(items.First(x => x.Id == v.Id), ESyncStatus.NotSynced, new Exception(reply.Data ?? ""), true);
                        result.Add(new DataLoadResult(){EntityId =v.Id,IsSuccess = false});
                    
                    }

                   // throw new DataLoadException(reply.Data);
                }
                else
                {
                    foreach (var v in batchList)
                    {
                        var i = items.First(x => x.Id == v.Id);
                        i.IsCreatedOnClient = false;
                        await new DbService().Save(i, ESyncStatus.Success, null, true);
                        result.Add(new DataLoadResult() { EntityId = v.Id, IsSuccess = true });
                    }

                    
                }
                counter += batchList.Count();
                Helpers.DebugMessage(string.Format("{0},Count - {1}-{3},Success={2}",typeof(T),counter,reply.IsSuccess,items.Count()));
         
            }

            return result;

        }


        async Task<List<T>> LoadWithPaging<T, V, S>(Func<int, Task<V>> loader, Func<V, List<S>> getItems) where V : BaseListReply
        {
            var reply = await LoadWithRetries(loader, 0);

            if (!reply.IsSuccess)
            {
                throw new DataLoadException();
            }

            var totalCount = reply.TotalCount;
            var result = new List<T>();
            int pages = totalCount / MyServiceClient.LOAD_ITEMS_PER_PAGE;

            if (totalCount == 0)
            {
                return result;
            }

            result.AddRange(Convert<T, S>(getItems(reply)));

            for (var i = 1; i <= pages; i++)
            {

                V r = await LoadWithRetries(loader, result.Count);

                if (r.IsSuccess)
                {
                    result.AddRange(Convert<T, S>(getItems(r)));
                }
              
            }

            return result;
        }

        async Task<V> LoadWithRetries<V>(Func<int, Task<V>> loader, int offset)where V : BaseListReply
        {
            int tries = 3;
            V r = null;
            while (tries > 0)
            {
                tries--;

                r = await loader(offset);

                if (r.IsSuccess)
                {
                    return r;
                }

            }
            throw new DataLoadException(r.Data);
        }


        public static List<T> Convert<T,K>(List<K> objects)
        {
            var r = new List<T>();

            foreach (var o in objects)
            {
                var e = AutoMapper.Mapper.Map<K, T>(o);
                r.Add(e);
            }

            return r;
        }
    }

    public class DataLoadException:Exception

    {
        public DataLoadException() : base()
        {
            
        }

        public DataLoadException(string message)
            : base(message,null)
        {

        }
    }

    public class DataLoadResult
    {
        public Guid EntityId { get; set; }

        public bool IsSuccess { get; set; }
    }
}


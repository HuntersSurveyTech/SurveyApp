using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetAddressesRequest : BaseAuthListRequest
    {
        public List<string> Customers { get; set; }

        public Guid? Id { get; set; }
    }

    public class GetAddressesReply : BaseListReply
    {
        public List<Address> Items { get; set; }
    }

    public class GetAddressesRequestHandler:RequestHandler<GetAddressesRequest,GetAddressesReply>
    {
        protected override GetAddressesReply Execute(GetAddressesRequest request)
        {
            try
            {
                DbContext.Configuration.AutoDetectChangesEnabled = false;

                var reply = CheckUser<GetAddressesReply>(request);

                if (reply != null) return reply;

                reply = new GetAddressesReply() {Items = new List<Address>()};

                if (request.Id != null)
                {
                    reply.TotalCount = 1;
                    reply.Items.Add(DbContext.Addresses.Find(request.Id));
                    return reply;
                }

                var user = DbContext.Surveyors.Find(request.UserAuthId);

                if (user.Type == (int) ESurveyorType.QA)
                {
                    var qaAddressesIds =
                        DbContext.QAAddresses.Where(x => !x.IsCompleted && x.SurveyorId == request.UserAuthId)
                            .Select(x => x.AddressId)
                            .ToList();

                    var qaAddresses = DbContext.Addresses.Where(x => qaAddressesIds.Contains(x.Id)).ToList();
                    reply.TotalCount = qaAddresses.Count;

                    reply.Items.AddRange(qaAddresses);

                    return reply;
                }


                var addresses =
                    DbContext.Addresses.Where(
                        x =>
                            x.SurveyorId == request.UserAuthId && (!x.IsCompleted || (x.IsCompleted && x.IsLoadToPhone)))
                        .Where(x => request.Customers.Contains(x.CustomerSurveyID))
                        .OrderBy(x => x.CreateDate);

                reply.TotalCount = addresses.Count();

                var items = addresses.Skip(request.Offset).Take(request.ItemsPerPage);

                reply.Items.AddRange(items);

                return reply;
            }
            finally
            {
                DbContext.Configuration.AutoDetectChangesEnabled = true;
            }


        }
    }

    public class GetAddressQuestionGroupStatusRequest : BaseAuthListRequest
    {
        public List<Guid> AddressIds { get; set; }
    }

    public class GetAddressQuestionGroupStatusReply : BaseListReply
    {
        public List<AddressQuestionGroupStatus> Items { get; set; }
    }

    public class GetAddressQuestionGroupStatusRequestHandler : RequestHandler<GetAddressQuestionGroupStatusRequest, GetAddressQuestionGroupStatusReply>
    {
        protected override GetAddressQuestionGroupStatusReply Execute(GetAddressQuestionGroupStatusRequest request)
        {
            var reply = CheckUser<GetAddressQuestionGroupStatusReply>(request);

            if (reply != null) return reply;

            reply = new GetAddressQuestionGroupStatusReply() { Items = new List<AddressQuestionGroupStatus>() };


            var items =
                DbContext.AddressQuestionGroupStatuses.Where(x => request.AddressIds.Contains(x.AddressId)).ToList();

            reply.Items.AddRange(items);

            return reply;

        }
    }

}
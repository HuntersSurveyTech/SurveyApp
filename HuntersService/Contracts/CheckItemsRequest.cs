using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class CheckItemsRequest:BaseAuthRequest 
    {
        public List<Guid>  Ids { get; set; }
        public string Type { get; set; }
    }

    public class CheckItemsReply : BaseReply
    {
        public List<Guid> Ids { get; set; }
    }

    public class CheckItemsRequestHandler : RequestHandler<CheckItemsRequest, CheckItemsReply>
    {
        protected override CheckItemsReply Execute(CheckItemsRequest request)
        {
            var reply = CheckUser<CheckItemsReply>(request);

            if (reply != null) return reply;

            reply = new CheckItemsReply(){Ids = new List<Guid>()};

            var type = Type.GetType(request.Type);
            
            var entities = DbContext.Set(type).Cast<Entity>().Where(x => request.Ids.Contains(x.Id)).OrderBy(x => x.CreateDate);

            foreach (var entity in entities)
            {
                reply.Ids.Add(entity.Id);
            }

            return reply;


        }
    }


}
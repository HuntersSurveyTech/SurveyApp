using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetRichMediasReply:BaseListReply
    {
        public List<RichMedia> Items { get; set; } 
    }

    public class GetRichMediasRequest : BaseAuthListRequest
    {
        public List<string> Customers { get; set; }


        public Guid? AddressId { get; set; }
    }

    public class GetRichMediasRequestHandler : RequestHandler<GetRichMediasRequest, GetRichMediasReply>
    {
        protected override GetRichMediasReply Execute(GetRichMediasRequest request)
        {
            var r = CheckUser<GetRichMediasReply>(request);

            if (r != null) return r;

            r = new GetRichMediasReply() { Items = new List<RichMedia>() };

            if (request.AddressId != null)
            {
                var ad = DbContext.Addresses.Find(request.AddressId);
                r.Items.AddRange(DbContext.RichMedias.Where(x=>x.UPRN ==ad.UPRN).ToList());
                r.TotalCount = r.Items.Count();

                return r;
            }

            var medias = DbContext.RichMedias.Where(x => request.Customers.Contains(x.CustomerSurveyID)).OrderBy(x => x.CreateDate);

            r.TotalCount = medias.Count();

            var items = medias.Skip(request.Offset).Take(request.ItemsPerPage).ToList();

            r.Items.AddRange(items);

            return r;

        }
    }
}
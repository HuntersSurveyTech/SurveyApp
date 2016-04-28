using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetSurvelemMapsReply:BaseListReply
    {
        public List<SurvelemMap> Items { get; set; } 
    }

    public class GetSurvelemMapsRequest : BaseAuthListRequest
    {
        public List<string> Customers { get; set; }
    }

    public class GetSurvelemMapsRequestHandler : RequestHandler<GetSurvelemMapsRequest, GetSurvelemMapsReply>
    {
        protected override GetSurvelemMapsReply Execute(GetSurvelemMapsRequest request)
        {
            var r = CheckUser<GetSurvelemMapsReply>(request);

            if (r != null) return r;

            r = new GetSurvelemMapsReply() { Items = new List<SurvelemMap>() };

            var maps = DbContext.SurvelemMaps.Where(x=>request.Customers.Contains(x.CustomerSurveyID)).OrderBy(x=>x.CreateDate);

            r.TotalCount = maps.Count();

            var items = maps.Skip(request.Offset).Take(request.ItemsPerPage).ToList();

            r.Items.AddRange(items);

            return r;

        }
    }
}
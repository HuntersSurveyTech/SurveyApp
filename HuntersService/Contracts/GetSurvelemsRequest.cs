using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetSurvelemsReply:BaseListReply
    {
        public List<Survelem> Items { get; set; } 
    }

    public class GetSurvelemsRequest : BaseAuthListRequest
    {
        public string Customer { get; set; }
        public string UPRN { get; set; }
    }

    public class GetSurvelemsRequestHandler : RequestHandler<GetSurvelemsRequest, GetSurvelemsReply>
    {
        protected override GetSurvelemsReply Execute(GetSurvelemsRequest request)
        {
            try
            {
                DbContext.Configuration.AutoDetectChangesEnabled = false;

                var r = CheckUser<GetSurvelemsReply>(request);

                if (r != null) return r;

                r = new GetSurvelemsReply() {Items = new List<Survelem>()};

                var maps =
                    DbContext.Survelems.Where(
                        x => x.CustomerSurveyID == request.Customer && x.UPRN == request.UPRN)
                        .OrderBy(x => x.CreateDate);

                r.TotalCount = maps.Count();

                var items = maps.Skip(request.Offset).Take(request.ItemsPerPage).ToList();

                r.Items.AddRange(items);

                return r;
            }
            finally
            {
                DbContext.Configuration.AutoDetectChangesEnabled = true;
            }
        }
    }
}
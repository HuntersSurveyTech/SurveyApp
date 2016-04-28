using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetOptionsReply:BaseListReply
    {
        public List<Option> Items { get; set; } 
    }

    public class GetOptionsRequest : BaseAuthListRequest
    {
        public List<string>  Customers { get; set; }
    }

    public class GetOptionsRequestHandler : RequestHandler<GetOptionsRequest, GetOptionsReply>
    {
        protected override GetOptionsReply Execute(GetOptionsRequest request)
        {
            var r = CheckUser<GetOptionsReply>(request);

            if (r != null) return r;

            r = new GetOptionsReply() { Items = new List<Option>() };

            var options = DbContext.Options.Where(x => request.Customers.Contains(x.CustomerSurveyID)).OrderBy(x => x.CreateDate);

            r.TotalCount = options.Count();

            var items = options.Skip(request.Offset).Take(request.ItemsPerPage).ToList();

            r.Items.AddRange(items);

            return r;

        }
    }
}
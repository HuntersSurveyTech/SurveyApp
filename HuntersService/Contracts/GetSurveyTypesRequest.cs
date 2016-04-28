using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetSurveyTypesReply : BaseReply
    {
        public List<SurveyType> Items { get; set; }
    }

    public class GetSurveyTypesRequest:BaseAuthRequest
    {
    }

    public class GetSurveyTypesRequestHandler : RequestHandler<GetSurveyTypesRequest, GetSurveyTypesReply>
    {
        protected override GetSurveyTypesReply Execute(GetSurveyTypesRequest request)
        {
            var r = CheckUser<GetSurveyTypesReply>(request);

            if (r != null) return r;

            r = new GetSurveyTypesReply(){Items = new List<SurveyType>()};
            
            var items = DbContext.SurveyTypes.ToList();

            r.Items.AddRange(items);

            return r;
        }
    }


   
}
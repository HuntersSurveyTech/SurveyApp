using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetQuestionsReply:BaseListReply
    {
        public List<Question> Items { get; set; } 
    }

    public class GetQuestionsRequest:BaseAuthListRequest
    {
        public List<string> Customers { get; set; }
    }

    public class GetQuestionsRequestHandler : RequestHandler<GetQuestionsRequest, GetQuestionsReply>
    {
        protected override GetQuestionsReply Execute(GetQuestionsRequest request)
        {
            var r = CheckUser<GetQuestionsReply>(request);

            if (r != null) return r;

            r = new GetQuestionsReply() { Items = new List<Question>() };

            var questions = DbContext.Questions.Where(x=>request.Customers.Contains(x.CustomerSurveyID)).OrderBy(x=>x.CreateDate);

            r.TotalCount = questions.Count();

            var items = questions.Skip(request.Offset).Take(request.ItemsPerPage).ToList();

            r.Items.AddRange(items);

            return r;

        }
    }
}
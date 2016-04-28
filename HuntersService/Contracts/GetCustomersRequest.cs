using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using HuntersService.Contracts.Base;
using HuntersService.Entities;

namespace HuntersService.Contracts
{
    public class GetCustomersRequest:BaseAuthRequest
    {
        
    }

    public class GetCustomersReply : BaseReply
    {
        public List<Customer> Items { get; set; }
    }

    public class GetCustomersRequestHandler : RequestHandler<GetCustomersRequest, GetCustomersReply>
    {
        protected override GetCustomersReply Execute(GetCustomersRequest request)
        {
            var reply = CheckUser<GetCustomersReply>(request);

            if (reply != null) return reply;

            reply = new GetCustomersReply() { Items = new List<Customer>() };

            var user = DbContext.Surveyors.Find(request.UserAuthId);

            var customers = DbContext.Customers.Where(x => !x.Completed).ToList();

            foreach (var customer in customers)
            {
                var ids = customer.Surveyors.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (ids.Contains(user.NetmeraId))
                {
                    reply.Items.Add(customer);
                }
            }


            return reply;
        }
    }
}
using HuntersService.Entities;

namespace HuntersService.Contracts.Base
{
	public class RequestContext
	{
        public MyDbContext DbContext { get; set; }

        public RequestContext(MyDbContext dbContext)
        {
            DbContext = dbContext;
        }


	}
}

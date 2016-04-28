using System;
using System.ServiceModel;

namespace HuntersService.Contracts.Base
{
	public class BaseRequest
	{

	}

    public class BaseAuthRequest:BaseRequest
    {

        public Guid UserAuthId { get; set; }
    }

    public class BaseAuthListRequest : BaseAuthRequest
    {
        public int Offset { get; set; }
        public int ItemsPerPage { get; set; }
    }
}

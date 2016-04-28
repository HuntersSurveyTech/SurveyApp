using System.ServiceModel;

namespace HuntersService.Contracts.Base
{
    public class BaseReply
    {
        public BaseReply()
        {
            IsSuccess = true;
        }

        [MessageBodyMember]
        public bool NotFoundUser { get; set; }

        [MessageBodyMember]
        public bool IsSuccess { get; set; }

        [MessageBodyMember]
        public string Data { get; set; }

    }

    public class BaseListReply:BaseReply
    {
        public int TotalCount { get; set; }
    }
}

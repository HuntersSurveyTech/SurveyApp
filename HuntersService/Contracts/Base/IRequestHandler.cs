namespace HuntersService.Contracts.Base
{
	public interface IRequestHandler
	{
		BaseReply Execute(BaseRequest request, RequestContext requestContext);
	}
}

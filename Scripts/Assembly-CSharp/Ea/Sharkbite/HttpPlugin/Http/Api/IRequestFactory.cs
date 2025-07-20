namespace Ea.Sharkbite.HttpPlugin.Http.Api
{
	public interface IRequestFactory
	{
		IRequest Resource(string uri);
	}
}

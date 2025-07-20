namespace Ea.Sharkbite.HttpPlugin.Http.Impl
{
	public class NimbleRequestFactory : DefaultRequestFactory
	{
		protected override DefaultRequest CreateRequest(string url)
		{
			return new NimbleRequest(url);
		}
	}
}

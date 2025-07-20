namespace Ea.Sharkbite.HttpPlugin.Http.Api
{
	public interface IResponsePostprocessor
	{
		void postprocess(IResponse response);
	}
}

namespace Ea.Sharkbite.HttpPlugin.Http.Api
{
	public interface IRequestPreprocessor
	{
		void preprocess(IRequest request);
	}
}

using System.Collections.Generic;

namespace Ea.Sharkbite.HttpPlugin.Http.Api
{
	public interface IResponse
	{
		string Body { get; set; }

		int Code { get; set; }

		IRequest Request { get; set; }

		long ContentLength { get; set; }

		int DownloadTime { get; set; }

		string ContentType { get; set; }

		IDictionary<string, string> Headers { get; set; }

		bool IsConnectionLost { get; set; }

		string Error { get; set; }

		bool Success { get; }

		IResponse WithPostprocessor(IResponsePostprocessor postprocessor);
	}
}

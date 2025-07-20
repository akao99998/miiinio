using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;

namespace Ea.Sharkbite.HttpPlugin.Http.Impl
{
	public class DefaultResponse : IResponse
	{
		public virtual string Body { get; set; }

		public virtual int Code { get; set; }

		public virtual IRequest Request { get; set; }

		public virtual long ContentLength { get; set; }

		public virtual int DownloadTime { get; set; }

		public virtual string ContentType { get; set; }

		public virtual IDictionary<string, string> Headers { get; set; }

		public virtual bool IsConnectionLost { get; set; }

		public virtual bool Success
		{
			get
			{
				return Code >= 200 && Code < 300 && string.IsNullOrEmpty(Error);
			}
		}

		public virtual string Error { get; set; }

		public DefaultResponse WithError(string error)
		{
			Error = error;
			return this;
		}

		public DefaultResponse WithBody(string body)
		{
			Body = body;
			return this;
		}

		public DefaultResponse WithCode(int code)
		{
			Code = code;
			return this;
		}

		public DefaultResponse WithRequest(IRequest request)
		{
			Request = request;
			return this;
		}

		public DefaultResponse WithContentLength(long contentLength)
		{
			ContentLength = contentLength;
			return this;
		}

		public DefaultResponse WithDownloadTime(int downloadTime)
		{
			DownloadTime = downloadTime;
			return this;
		}

		public DefaultResponse WithContentType(string contentType)
		{
			ContentType = contentType;
			return this;
		}

		public DefaultResponse WithHeaders(IDictionary<string, string> headers)
		{
			Headers = headers;
			return this;
		}

		public DefaultResponse WithConnectionLoss(bool isConnectionLost)
		{
			IsConnectionLost = isConnectionLost;
			return this;
		}

		public IResponse WithPostprocessor(IResponsePostprocessor postprocessor)
		{
			postprocessor.postprocess(this);
			return this;
		}
	}
}

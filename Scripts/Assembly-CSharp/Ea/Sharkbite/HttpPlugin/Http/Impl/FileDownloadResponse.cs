using System;

namespace Ea.Sharkbite.HttpPlugin.Http.Impl
{
	public class FileDownloadResponse : DefaultResponse
	{
		public override string Body
		{
			get
			{
				throw new NotImplementedException("Download requests do not have a body.  The downloaded contents were written to the specified file.");
			}
			set
			{
				throw new NotImplementedException("Download requests do not have a body.  The downloaded contents were written to the specified file.");
			}
		}
	}
}

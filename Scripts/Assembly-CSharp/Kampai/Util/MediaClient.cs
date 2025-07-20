using System;
using System.Runtime.InteropServices;

namespace Kampai.Util
{
	public static class MediaClient
	{
		[DllImport("media_client")]
		public static extern int media_client_start(IntPtr log_handler, int log_severity);

		[DllImport("media_client")]
		public static extern string media_client_convert_url(string url);

		[DllImport("media_client")]
		public static extern string media_client_stop();

		public static void Start()
		{
			media_client_start(IntPtr.Zero, 0);
		}

		public static string ConvertUrl(string url)
		{
			url = url.Replace("https://", "http://");
			return media_client_convert_url(url);
		}

		public static void Stop()
		{
			Native.LogInfo(string.Format("MediaClient.Stop: {0}", media_client_stop()));
		}
	}
}

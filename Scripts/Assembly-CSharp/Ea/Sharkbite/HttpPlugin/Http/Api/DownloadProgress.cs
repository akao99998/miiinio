using System.Diagnostics;

namespace Ea.Sharkbite.HttpPlugin.Http.Api
{
	public class DownloadProgress
	{
		private Stopwatch stopWatch = new Stopwatch();

		public string Uri { get; private set; }

		public long TotalBytes { get; set; }

		public long CompletedBytes { get; set; }

		public long DownloadedBytes { get; set; }

		public long Delta { get; set; }

		public bool IsGZipped { get; set; }

		public float CompressionRatio { get; set; }

		public DownloadProgress(string uri)
		{
			Uri = uri;
			CompressionRatio = 1f;
		}

		public float GetProgress()
		{
			if (TotalBytes == 0L)
			{
				return 0f;
			}
			return (float)CompletedBytes / (float)TotalBytes;
		}

		public void StartTimer()
		{
			stopWatch.Start();
		}

		public void StopTimer()
		{
			stopWatch.Stop();
		}

		public int GetDownloadTime()
		{
			return (int)stopWatch.ElapsedMilliseconds;
		}

		public bool IsRunning()
		{
			return stopWatch.IsRunning;
		}

		public DownloadProgress Clone()
		{
			DownloadProgress downloadProgress = new DownloadProgress(Uri);
			downloadProgress.TotalBytes = TotalBytes;
			downloadProgress.CompletedBytes = CompletedBytes;
			downloadProgress.DownloadedBytes = DownloadedBytes;
			downloadProgress.Delta = Delta;
			downloadProgress.IsGZipped = IsGZipped;
			downloadProgress.CompressionRatio = CompressionRatio;
			downloadProgress.stopWatch = stopWatch;
			return downloadProgress;
		}
	}
}

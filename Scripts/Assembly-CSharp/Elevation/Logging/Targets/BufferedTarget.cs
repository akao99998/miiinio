using System.IO;

namespace Elevation.Logging.Targets
{
	public abstract class BufferedTarget : FileTarget
	{
		public BufferedTarget(string name, LogLevel level, int timeoutMillis, int bufferSize, string logFolder = null, params LogFilter[] filters)
			: base(level, string.Format("{0}.buffer", name), bufferSize, 3, logFolder, filters)
		{
			base.TimeoutMillis = timeoutMillis;
			base.Name = name;
		}

		protected override void RollLogFiles()
		{
			FileInfo currentFileInfo = _currentFileInfo;
			base.RollLogFiles();
			if (currentFileInfo != null)
			{
				BatchProcess(currentFileInfo);
			}
		}

		protected abstract void BatchProcess(FileInfo buffer);

		public override void Flush()
		{
			if (!base.Disposed)
			{
				base.Flush();
				RollLogFiles();
			}
		}
	}
}

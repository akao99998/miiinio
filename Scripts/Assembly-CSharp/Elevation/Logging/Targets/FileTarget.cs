using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Elevation.Logging.Targets
{
	public class FileTarget : AsyncLoggingTarget
	{
		private StreamWriter _writer;

		private readonly string _logFolder;

		private readonly string _filePrefix;

		private readonly int _maxFileSize;

		private readonly int _maxFileCount;

		protected FileInfo _currentFileInfo;

		private DirectoryInfo _logFolderInfo;

		private readonly HashSet<string> _previousFiles = new HashSet<string>();

		protected int CurrentFileSize
		{
			get
			{
				if (_writer != null)
				{
					return (int)_writer.BaseStream.Length;
				}
				return 0;
			}
		}

		private DirectoryInfo LogFolderInfo
		{
			get
			{
				if (_logFolderInfo != null)
				{
					return _logFolderInfo;
				}
				_logFolderInfo = new DirectoryInfo(_logFolder);
				if (!_logFolderInfo.Exists)
				{
					_logFolderInfo.Create();
				}
				return _logFolderInfo;
			}
		}

		private FileInfo CurrentLogFile
		{
			get
			{
				if (_currentFileInfo != null)
				{
					return _currentFileInfo;
				}
				FileInfo[] files = LogFolderInfo.GetFiles(string.Format("{0}*.log", _filePrefix));
				Array.Sort(files, (FileInfo f1, FileInfo f2) => string.Compare(f2.Name, f1.Name, StringComparison.Ordinal));
				FileInfo fileInfo = null;
				for (int i = 0; i < files.Length; i++)
				{
					if (!_previousFiles.Contains(files[i].FullName))
					{
						fileInfo = files[i];
						break;
					}
				}
				if (fileInfo != null && fileInfo.Length <= _maxFileSize)
				{
					_currentFileInfo = fileInfo;
				}
				else
				{
					DateTime utcNow = DateTime.UtcNow;
					int num = 1;
					FileInfo fileInfo2 = null;
					while (fileInfo2 == null)
					{
						string arg = string.Format("{0}/{1}_{2}-{3:00}-{4:00}T{5:00}-{6:00}-{7:00}", LogFolderInfo.FullName, _filePrefix, utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second);
						fileInfo2 = new FileInfo(string.Format("{0}.log", arg));
						if (!fileInfo2.Exists)
						{
							break;
						}
						fileInfo2 = new FileInfo(string.Format("{0}_{1:000}.log", arg, utcNow.Millisecond));
						if (!fileInfo2.Exists)
						{
							break;
						}
						fileInfo2 = new FileInfo(string.Format("{0}_{1:000}_{2}.log", arg, utcNow.Millisecond, num));
						if (!fileInfo2.Exists)
						{
							break;
						}
						fileInfo2 = null;
						num++;
					}
					_currentFileInfo = fileInfo2;
					_previousFiles.Add(_currentFileInfo.FullName);
				}
				return _currentFileInfo;
			}
		}

		public FileTarget(LogLevel level, string filePrefix, int maxFileSize, int maxFileCount, string logFolder = null, params LogFilter[] filters)
			: base("File", level, filters)
		{
			_logFolder = ((logFolder != null) ? logFolder : string.Format("{0}/Logs", Application.temporaryCachePath));
			_filePrefix = filePrefix;
			_maxFileSize = ((maxFileSize <= 0) ? 1048576 : maxFileSize);
			_maxFileCount = ((maxFileCount <= 0) ? 1 : maxFileCount);
		}

		public static FileTarget Build(Dictionary<string, object> config)
		{
			string logFolder = null;
			string filePrefix = "elevation";
			int maxFileSize = 5242880;
			int maxFileCount = 2;
			if (config.ContainsKey("logFolder"))
			{
				logFolder = config["logFolder"].ToString();
			}
			if (config.ContainsKey("filePrefix"))
			{
				filePrefix = config["filePrefix"].ToString();
			}
			if (config.ContainsKey("maxFileSize"))
			{
				maxFileSize = int.Parse(config["maxFileSize"].ToString());
			}
			if (config.ContainsKey("maxFileCount"))
			{
				maxFileCount = int.Parse(config["maxFileCount"].ToString());
			}
			FileTarget fileTarget = new FileTarget(LogLevel.None, filePrefix, maxFileSize, maxFileCount, logFolder);
			fileTarget.UpdateConfig(config);
			return fileTarget;
		}

		protected override void Write(LogEvent logEvent)
		{
			try
			{
				if (_writer == null)
				{
					_writer = new StreamWriter(CurrentLogFile.FullName, true, Encoding.UTF8, 32768)
					{
						AutoFlush = true,
						NewLine = "\n"
					};
				}
				_writer.WriteLine(FormattedLogEvent(logEvent));
				if (_writer.BaseStream.Length > _maxFileSize)
				{
					RollLogFiles();
				}
			}
			catch (Exception arg)
			{
				Console.Error.WriteLine("Unable to write to log file: {0}\n{1}", _currentFileInfo, arg);
				base.Level = LogLevel.None;
			}
		}

		protected virtual void RollLogFiles()
		{
			if (_writer == null)
			{
				return;
			}
			_writer.Flush();
			_writer.Close();
			_writer = null;
			_currentFileInfo = null;
			FileInfo[] files = LogFolderInfo.GetFiles(string.Format("{0}*.log", _filePrefix));
			if (files.Length >= _maxFileCount)
			{
				Array.Sort(files, (FileInfo f1, FileInfo f2) => string.Compare(f1.Name, f2.Name, StringComparison.Ordinal));
				for (int i = 0; i <= files.Length - _maxFileCount; i++)
				{
					_previousFiles.Remove(files[i].FullName);
					files[i].Delete();
				}
			}
		}
	}
}

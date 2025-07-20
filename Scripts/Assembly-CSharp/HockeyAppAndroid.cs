using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;

public class HockeyAppAndroid : MonoBehaviour
{
	protected const string HOCKEYAPP_BASEURL = "https://rink.hockeyapp.net/api/2/apps/";

	protected const string HOCKEYAPP_CRASHESPATH = "/crashes/upload";

	protected const int MAX_CHARS = 199800;

	protected const string LOG_FILE_DIR = "/logs/";

	protected const string DATE_LABEL = "Date:";

	protected const string TELEMETRY_DATE_FORMAT = "yyyymmdd_HHmmss";

	protected const string LOG_DATE_FORMAT = "ddd MMM dd HH:mm:ss {}zzzz yyyy";

	protected const string LOCAL_TIMEZONE = "GMT";

	public string appID = string.Empty;

	public string packageID = string.Empty;

	public bool exceptionLogging;

	public bool autoUpload;

	public bool updateManager;

	public string userId = string.Empty;

	public Action crashReportCallback;

	internal ITelemetryService telemetryService;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (exceptionLogging)
		{
			List<string> logFiles = GetLogFiles();
			if (logFiles.Count > 0)
			{
				StartCoroutine(SendLogs(GetLogFiles()));
			}
		}
		StartCrashManager(appID, updateManager, autoUpload, userId);
	}

	private void OnEnable()
	{
		if (exceptionLogging)
		{
			AppDomain.CurrentDomain.UnhandledException += OnHandleUnresolvedException;
			Application.logMessageReceived += OnHandleLogCallback;
		}
	}

	private void OnDisable()
	{
		if (exceptionLogging)
		{
			AppDomain.CurrentDomain.UnhandledException -= OnHandleUnresolvedException;
			Application.logMessageReceived -= OnHandleLogCallback;
		}
	}

	protected void StartCrashManager(string appID, bool updateManagerEnabled, bool autoSendEnabled, string userID)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("net.hockeyapp.unity.HockeyUnityPlugin");
		androidJavaClass2.CallStatic("startHockeyAppManager", appID, @static, updateManagerEnabled, autoSendEnabled, userID);
	}

	protected string GetVersion()
	{
		string text = null;
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("net.hockeyapp.unity.HockeyUnityPlugin");
		return androidJavaClass.CallStatic<string>("getAppVersion", new object[0]);
	}

	protected virtual List<string> GetLogHeaders()
	{
		List<string> list = new List<string>();
		list.Add("Package: " + packageID);
		string version = GetVersion();
		list.Add("Version: " + version);
		string[] array = SystemInfo.operatingSystem.Split('/');
		string item = "Android: " + array[0].Replace("Android OS ", string.Empty);
		list.Add(item);
		list.Add("Model: " + SystemInfo.deviceModel);
		list.Add("GPU vendor: " + SystemInfo.graphicsDeviceVendor);
		list.Add("GPU name: " + SystemInfo.graphicsDeviceName);
		list.Add("VRAM: " + SystemInfo.graphicsMemorySize);
		list.Add("RAM: " + SystemInfo.systemMemorySize);
		list.Add("Date: " + DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss {}zzzz yyyy").Replace("{}", "GMT"));
		return list;
	}

	protected virtual WWWForm CreateForm(string log)
	{
		WWWForm wWWForm = new WWWForm();
		byte[] array = null;
		using (FileStream fileStream = File.OpenRead(log))
		{
			if (fileStream.Length > 199800)
			{
				string text = null;
				using (StreamReader streamReader = new StreamReader(fileStream))
				{
					streamReader.BaseStream.Seek(fileStream.Length - 199800, SeekOrigin.Begin);
					text = streamReader.ReadToEnd();
				}
				List<string> logHeaders = GetLogHeaders();
				string text2 = string.Empty;
				foreach (string item in logHeaders)
				{
					text2 = text2 + item + "\n";
				}
				text = text2 + "\n[...]" + text;
				try
				{
					array = Encoding.Default.GetBytes(text);
				}
				catch (ArgumentException ex)
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log("Failed to read bytes of log file: " + ex);
					}
				}
			}
			else
			{
				try
				{
					array = File.ReadAllBytes(log);
				}
				catch (SystemException ex2)
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log("Failed to read bytes of log file: " + ex2);
					}
				}
			}
		}
		if (array != null)
		{
			wWWForm.AddBinaryData("log", array, log, "text/plain");
			string logFilesContent = Native.GetLogFilesContent(32768);
			if (!string.IsNullOrEmpty(logFilesContent))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(logFilesContent);
				if (bytes != null)
				{
					wWWForm.AddBinaryData("description", bytes, "description", "text/plain");
				}
			}
		}
		wWWForm.AddField("userID", userId);
		return wWWForm;
	}

	protected virtual List<string> GetLogFiles()
	{
		List<string> list = new List<string>();
		string path = Application.persistentDataPath + "/logs/";
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			FileInfo[] files = directoryInfo.GetFiles();
			if (files.Length > 0)
			{
				FileInfo[] array = files;
				foreach (FileInfo fileInfo in array)
				{
					if (fileInfo.Extension == ".log")
					{
						list.Add(fileInfo.FullName);
					}
					else
					{
						File.Delete(fileInfo.FullName);
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Failed to write exception log to file: " + ex);
			}
		}
		return list;
	}

	protected virtual IEnumerator SendLogs(List<string> logs)
	{
		foreach (string log in logs)
		{
			string url = "https://rink.hockeyapp.net/api/2/apps/" + appID + "/crashes/upload";
			WWWForm postForm = CreateForm(log);
			if (postForm.data != null)
			{
				SendCrashTelemetry(Encoding.UTF8.GetString(postForm.data));
			}
			try
			{
				File.Delete(log);
			}
			catch (Exception e)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log("Failed to delete exception log: " + e);
				}
			}
			string lContent2 = postForm.headers["Content-Type"].ToString();
			lContent2 = lContent2.Replace("\"", string.Empty);
			yield return new WWW(headers: new Dictionary<string, string> { { "Content-Type", lContent2 } }, url: url, postData: postForm.data);
			if (crashReportCallback != null)
			{
				crashReportCallback();
			}
		}
	}

	protected void SendCrashTelemetry(string crashData)
	{
		if (telemetryService == null || crashData == null)
		{
			return;
		}
		string[] array = crashData.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = string.Empty;
		string[] array2 = array;
		foreach (string text5 in array2)
		{
			if (text5.All(char.IsWhiteSpace))
			{
				continue;
			}
			if (text == null)
			{
				int num = text5.IndexOf("Date:");
				if (num != -1)
				{
					text = ConvertDateTimeForTelemetry(text5.Substring(num + "Date:".Length).TrimStart().TrimEnd());
				}
			}
			else if (text2 == null)
			{
				int length = text5.IndexOf(":");
				text2 = text5.Substring(0, length);
				text4 = text5;
			}
			else
			{
				if (text3 == null)
				{
					text3 = text5;
				}
				text4 = text4 + "\n" + text5;
			}
		}
		if (text2 != null)
		{
			telemetryService.Send_Telemetry_EVT_GAME_ERROR_CRASH(text2, text3, text, text4);
		}
	}

	protected string ConvertDateTimeForTelemetry(string crashTime)
	{
		if (crashTime != null)
		{
			crashTime = crashTime.Replace("GMT", string.Empty);
			string text = "ddd MMM dd HH:mm:ss {}zzzz yyyy".Replace("{}", string.Empty);
			string[] formats = new string[1] { text };
			DateTime result;
			if (DateTime.TryParseExact(crashTime, formats, new CultureInfo("en-US"), DateTimeStyles.None, out result))
			{
				crashTime = result.ToString("yyyymmdd_HHmmss");
			}
		}
		return crashTime;
	}

	protected virtual void WriteLogToDisk(string logString, string stackTrace)
	{
		string text = DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss_fff");
		string text2 = logString.Replace("\n", " ");
		string[] array = stackTrace.Split('\n');
		text2 = "\n" + text2 + "\n";
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			if (text3.Length > 0)
			{
				text2 = text2 + "  at " + text3 + "\n";
			}
		}
		List<string> logHeaders = GetLogHeaders();
		using (StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/logs/LogFile_" + text + ".log", true))
		{
			foreach (string item in logHeaders)
			{
				streamWriter.WriteLine(item);
			}
			streamWriter.WriteLine(text2);
			Native.AndroidFileLog(text2);
		}
	}

	protected virtual void HandleException(string logString, string stackTrace)
	{
		WriteLogToDisk(logString, stackTrace);
	}

	public void OnHandleLogCallback(string logString, string stackTrace, LogType type)
	{
		if (exceptionLogging && (type == LogType.Assert || type == LogType.Exception) && (type != LogType.Exception || !logString.StartsWith("FatalException")))
		{
			exceptionLogging = false;
			AppDomain.CurrentDomain.UnhandledException -= OnHandleUnresolvedException;
			Application.logMessageReceived -= OnHandleLogCallback;
			HandleException(logString, stackTrace);
			Native.CloseFileLog();
			Application.Quit();
		}
	}

	public void OnHandleUnresolvedException(object sender, UnhandledExceptionEventArgs args)
	{
		if (exceptionLogging && args != null && args.ExceptionObject != null && args.ExceptionObject.GetType() == typeof(Exception))
		{
			exceptionLogging = false;
			AppDomain.CurrentDomain.UnhandledException -= OnHandleUnresolvedException;
			Application.logMessageReceived -= OnHandleLogCallback;
			Exception ex = (Exception)args.ExceptionObject;
			HandleException(ex.Source, ex.StackTrace);
			Native.CloseFileLog();
			Application.Quit();
		}
	}
}

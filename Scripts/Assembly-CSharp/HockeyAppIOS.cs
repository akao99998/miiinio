using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kampai.Common;
using UnityEngine;

public class HockeyAppIOS : MonoBehaviour
{
	protected const string HOCKEYAPP_BASEURL = "https://rink.hockeyapp.net/";

	protected const string HOCKEYAPP_CRASHESPATH = "api/2/apps/[APPID]/crashes/upload";

	protected const int MAX_CHARS = 199800;

	protected const string LOG_FILE_DIR = "/logs/";

	protected const string DATE_LABEL = "Date:";

	protected const string TELEMETRY_DATE_FORMAT = "yyyymmdd_HHmmss";

	protected const string LOG_DATE_FORMAT = "ddd MMM dd HH:mm:ss {}zzzz yyyy";

	protected const string LOCAL_TIMEZONE = "GMT";

	public string appID = string.Empty;

	public string secret = string.Empty;

	public string authenticationType = string.Empty;

	public string serverURL = string.Empty;

	public bool autoUpload;

	public bool exceptionLogging;

	public bool updateManager;

	public string userId = string.Empty;

	public Action crashReportCallback;

	internal ITelemetryService telemetryService;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void GameViewLoaded(string message)
	{
	}

	protected virtual List<string> GetLogHeaders()
	{
		return new List<string>();
	}

	protected virtual WWWForm CreateForm(string log)
	{
		return new WWWForm();
	}

	protected virtual List<string> GetLogFiles()
	{
		return new List<string>();
	}

	protected virtual IEnumerator SendLogs(List<string> logs)
	{
		string crashPath = "api/2/apps/[APPID]/crashes/upload";
		string url = GetBaseURL() + crashPath.Replace("[APPID]", appID);
		foreach (string log in logs)
		{
			WWWForm postForm = CreateForm(log);
			if (postForm.data != null)
			{
				SendCrashTelemetry(Encoding.UTF8.GetString(postForm.data));
			}
			string lContent2 = postForm.headers["Content-Type"].ToString();
			lContent2 = lContent2.Replace("\"", string.Empty);
			WWW www = new WWW(headers: new Dictionary<string, string> { { "Content-Type", lContent2 } }, url: url, postData: postForm.data);
			yield return www;
			if (string.IsNullOrEmpty(www.error))
			{
				try
				{
					File.Delete(log);
				}
				catch (Exception ex)
				{
					Exception e = ex;
					if (Debug.isDebugBuild)
					{
						Debug.Log("Failed to delete exception log: " + e);
					}
				}
			}
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
	}

	protected virtual string GetBaseURL()
	{
		return string.Empty;
	}

	protected virtual bool IsConnected()
	{
		return false;
	}

	protected virtual void HandleException(string logString, string stackTrace)
	{
	}

	public void OnHandleLogCallback(string logString, string stackTrace, LogType type)
	{
	}

	public void OnHandleUnresolvedException(object sender, UnhandledExceptionEventArgs args)
	{
	}
}

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Kampai.Util
{
	public static class Native
	{
		private enum LogLevel
		{
			ANDROID_LOG_VERBOSE = 2,
			ANDROID_LOG_DEBUG = 3,
			ANDROID_LOG_INFO = 4,
			ANDROID_LOG_WARN = 5,
			ANDROID_LOG_ERROR = 6,
			ANDROID_LOG_FATAL = 7
		}

		public class OnReceiveBroadcastListener : AndroidJavaProxy
		{
			public delegate void OnReceiveBroadcast(AndroidJavaObject context, AndroidJavaObject intent);

			private static readonly string NATIVE_INTERFACE = string.Format("com.ea.gp.minions.app.{0}", typeof(OnReceiveBroadcastListener).Name);

			private OnReceiveBroadcast broadcastListener;

			public OnReceiveBroadcastListener(OnReceiveBroadcast listener)
				: base(NATIVE_INTERFACE)
			{
				broadcastListener = listener;
			}

			public void onReceiveBroadcast(AndroidJavaObject context, AndroidJavaObject intent)
			{
				if (broadcastListener != null)
				{
					broadcastListener(context, intent);
					return;
				}
				context.Dispose();
				intent.Dispose();
			}
		}

		private const string MINIONS_LIB_NAME = "minions";

		private static string bundleVersion = null;

		private static string bundleIdentifier = null;

		private static StreamWriter sw = null;

		private static int androidOSVersion = 0;

		private static readonly long APP_START_TIME = getAppStartTime();

		public static string StaticConfig
		{
			get
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
					string text = @static.Call<string>("getPackageName", new object[0]);
					AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getResources", new object[0]);
					int num = androidJavaObject.Call<int>("getIdentifier", new object[3] { "config", "raw", text });
					StringBuilder stringBuilder = new StringBuilder();
					AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("openRawResource", new object[1] { num });
					AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("java.io.InputStreamReader", androidJavaObject2);
					AndroidJavaObject androidJavaObject4 = new AndroidJavaObject("java.io.BufferedReader", androidJavaObject3);
					string value;
					while ((value = androidJavaObject4.Call<string>("readLine", new object[0])) != null)
					{
						stringBuilder.Append(value);
					}
					return stringBuilder.ToString();
				}
			}
		}

		public static string BundleVersion
		{
			get
			{
				if (bundleVersion == null)
				{
					using (AndroidJavaObject androidJavaObject = GetCurrentActivity())
					{
						using (AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getPackageManager", new object[0]))
						{
							using (AndroidJavaObject androidJavaObject3 = androidJavaObject2.Call<AndroidJavaObject>("getPackageInfo", new object[2] { BundleIdentifier, 0 }))
							{
								bundleVersion = androidJavaObject3.Get<string>("versionName");
							}
						}
					}
				}
				return bundleVersion;
			}
		}

		public static string BundleIdentifier
		{
			get
			{
				if (bundleIdentifier == null)
				{
					using (AndroidJavaObject androidJavaObject = GetCurrentActivity())
					{
						bundleIdentifier = androidJavaObject.Call<string>("getPackageName", new object[0]);
					}
				}
				return bundleIdentifier;
			}
		}

		[DllImport("minions")]
		private static extern int Minions_Util_Native_Log(int logLevel, string tag, string msg);

		public static bool IsUserMusicPlaying()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.MainActivity"))
			{
				return androidJavaClass.CallStatic<bool>("isUserMusicPlaying", new object[0]);
			}
		}

		public static void LogError(string text)
		{
			Minions_Util_Native_Log(6, "Minions", text);
			AndroidFileLog(text);
		}

		public static void LogWarning(string text)
		{
			Minions_Util_Native_Log(5, "Minions", text);
			AndroidFileLog(text);
		}

		public static void LogInfo(string text)
		{
			Minions_Util_Native_Log(4, "Minions", text);
			AndroidFileLog(text);
		}

		public static void LogDebug(string text)
		{
			Minions_Util_Native_Log(3, "Minions", text);
			AndroidFileLog(text);
		}

		public static void LogVerbose(string text)
		{
			Minions_Util_Native_Log(2, "Minions", text);
			AndroidFileLog(text);
		}

		public static string GetFilePath()
		{
			string empty = string.Empty;
			return GameConstants.PERSISTENT_DATA_PATH + "/log.txt";
		}

		public static string GetLastFilePath()
		{
			return GameConstants.PERSISTENT_DATA_PATH + "/log-last.txt";
		}

		public static string GetLogFilesContent(int maxsize)
		{
			string result = string.Empty;
			string lastFilePath = GetLastFilePath();
			if (File.Exists(lastFilePath))
			{
				string text = File.ReadAllText(lastFilePath);
				result = ((text.Length <= maxsize) ? text : text.Substring(text.Length - maxsize));
			}
			return result;
		}

		public static uint GetMemoryUsage()
		{
			return Profiler.usedHeapSize;
		}

		public static void Crash()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.Misc"))
			{
				androidJavaClass.CallStatic("crash");
			}
		}

		public static void ScheduleLocalNotification(string type, int secondsFromNow, string title, string text, string stackedTitle, string stackedText, string action, string sound, string launchImage, int badgeNumber = 1)
		{
			using (AndroidJavaClass androidJavaClass = GetNotificationManagerHelper())
			{
				androidJavaClass.CallStatic("scheduleNotification", (long)TimeSpan.FromSeconds(secondsFromNow).TotalMilliseconds, type, launchImage, sound, title, text, stackedTitle, stackedText, badgeNumber);
			}
		}

		public static void CancelLocalNotification(string type)
		{
			using (AndroidJavaClass androidJavaClass = GetNotificationManagerHelper())
			{
				androidJavaClass.CallStatic("cancelNotification", type, false);
			}
		}

		public static void CancelAllLocalNotifications()
		{
			using (AndroidJavaClass androidJavaClass = GetNotificationManagerHelper())
			{
				androidJavaClass.CallStatic("cancelAllNotifications");
			}
		}

		public static string GetDeviceLanguage()
		{
			string text = "en";
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("java.util.Locale"))
			{
				using (AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("getDefault", new object[0]))
				{
					return androidJavaObject.Call<string>("toString", new object[0]);
				}
			}
		}

		public static bool AutorotationIsOSAllowed()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.Misc"))
			{
				int num = androidJavaClass.CallStatic<int>("getAutorotateSetting", new object[0]);
				return num != 0;
			}
		}

		public static bool GetBackupFlag(string path)
		{
			return false;
		}

		public static void SetBackupFlag(string path, bool shouldBackup)
		{
		}

		public static void AndroidFileLog(string text)
		{
			string filePath = GetFilePath();
			if (sw == null)
			{
				if (!File.Exists(filePath))
				{
					sw = File.CreateText(filePath);
				}
				else
				{
					sw = File.AppendText(filePath);
				}
			}
			if (sw != null)
			{
				sw.WriteLine(text + "\n");
			}
		}

		public static void CloseFileLog()
		{
			sw.Close();
			sw = null;
			if (File.Exists(GetLastFilePath()))
			{
				File.Delete(GetLastFilePath());
			}
			File.Copy(GetFilePath(), GetLastFilePath());
		}

		public static int GetAndroidOSVersion()
		{
			if (androidOSVersion == 0)
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build$VERSION"))
				{
					androidOSVersion = androidJavaClass.GetStatic<int>("SDK_INT");
				}
			}
			return androidOSVersion;
		}

		public static AndroidJavaObject GetCurrentActivity()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				return androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			}
		}

		public static AndroidJavaClass GetNotificationManagerHelper()
		{
			return new AndroidJavaClass("com.ea.gp.minions.notifications.NotificationManagerHelper");
		}

		public static string GetPersistentDataPath()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.FileUtils"))
			{
				return androidJavaClass.CallStatic<string>("getPersistentDataPath", new object[0]);
			}
		}

		public static ulong GetAvailableStorage(string path)
		{
			ulong num = 0uL;
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.FileUtils"))
			{
				return (ulong)((!string.IsNullOrEmpty(path)) ? androidJavaClass.CallStatic<long>("getAvailableStorage", new object[1] { path }) : androidJavaClass.CallStatic<long>("getAvailableInternalStorage", new object[0]));
			}
		}

		public static bool CanShowNetworkSettings()
		{
			return true;
		}

		public static void OpenNetworkSettings()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.Misc"))
			{
				androidJavaClass.CallStatic("openNetworkSettings");
			}
		}

		public static byte[] GetStreamingAsset(string path)
		{
			byte[] result = null;
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.FileUtils"))
			{
				try
				{
					result = androidJavaClass.CallStatic<byte[]>("openAsset", new object[1] { path });
				}
				catch (AndroidJavaException ex)
				{
					AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("java.io.FileNotFoundException");
					if (androidJavaClass2.GetType().IsInstanceOfType(ex))
					{
						throw new FileNotFoundException(ex.ToString());
					}
					LogError("Error opening asset: " + ex.ToString());
				}
			}
			return result;
		}

		public static string GetStreamingTextAsset(string path)
		{
			string result = null;
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.FileUtils"))
			{
				try
				{
					result = androidJavaClass.CallStatic<string>("openTextAsset", new object[1] { path });
				}
				catch (AndroidJavaException ex)
				{
					AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("java.io.FileNotFoundException");
					if (androidJavaClass2.GetType().IsInstanceOfType(ex))
					{
						throw new FileNotFoundException(ex.ToString());
					}
					LogError("Error opening text asset: " + ex.ToString());
				}
			}
			return result;
		}

		public static void Exit()
		{
			using (AndroidJavaObject androidJavaObject = GetCurrentActivity())
			{
				androidJavaObject.Call<bool>("moveTaskToBack", new object[1] { true });
			}
		}

		public static bool AreNotificationsEnabled()
		{
			if (GetAndroidOSVersion() >= 19)
			{
				using (AndroidJavaClass androidJavaClass = GetNotificationManagerHelper())
				{
					return androidJavaClass.CallStatic<bool>("isNotificationEnabled", new object[0]);
				}
			}
			return true;
		}

		public static string GetDeviceHardwareModel()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build"))
			{
				return androidJavaClass.GetStatic<string>("HARDWARE");
			}
		}

		private static long getAppStartTime()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.MinionsApplication"))
			{
				return androidJavaClass.GetStatic<long>("APP_START_TIME");
			}
		}

		public static long GetAppStartupTime()
		{
			return APP_START_TIME;
		}

		public static void OpenAppStoreLink(string appId)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.ea.gp.minions.utils.Misc"))
			{
				androidJavaClass.CallStatic("OpenAppStoreLink", appId);
			}
		}

		public static bool IsAppInstalled(string appIdURL)
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getPackageManager", new object[0]);
			try
			{
				androidJavaObject.Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { appIdURL });
				return true;
			}
			catch (Exception)
			{
			}
			return false;
		}

		public static void LaunchApp(string appId)
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getPackageManager", new object[0]);
			try
			{
				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { appId });
				@static.Call("startActivity", androidJavaObject2);
			}
			catch (Exception)
			{
				LogWarning("Error attempting to launch App.");
			}
		}

		public static bool CanOpenURL(string URL)
		{
			return false;
		}

		public static void OpenURL(string URL)
		{
			Application.OpenURL(URL);
		}
	}
}

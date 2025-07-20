using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class TimeService : ITimeService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("TimeService") as IKampaiLogger;

		private int serverTimestamp = int.MinValue;

		private int syncTime;

		private int appTickCount;

		private DateTime startTime;

		public TimeService()
		{
			startTime = Process.GetCurrentProcess().StartTime;
			new Timer(delegate
			{
				appTickCount++;
			}, null, 1000, 1000);
		}

		public int CurrentTime()
		{
			if (serverTimestamp == int.MinValue)
			{
				return ClientTime();
			}
			return ServerTime();
		}

		public int ServerTime()
		{
			return serverTimestamp + (Uptime() - syncTime);
		}

		public int ClientTime()
		{
			return Convert.ToInt32((DateTime.UtcNow - GameConstants.Timers.epochStart).TotalSeconds);
		}

		public int Uptime()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.SystemClock"))
			{
				long num = androidJavaClass.CallStatic<long>("elapsedRealtime", new object[0]);
				return Convert.ToInt32(num / 1000);
			}
		}

		public int AppTime()
		{
			return appTickCount;
		}

		public void SyncServerTime(IResponse response)
		{
			IDictionary<string, string> headers = response.Headers;
			if (headers != null && headers.ContainsKey("Date"))
			{
				string text = headers["Date"];
				DateTime result;
				if (DateTime.TryParse(text, out result))
				{
					serverTimestamp = Convert.ToInt32((result.ToUniversalTime() - GameConstants.Timers.epochStart).TotalSeconds);
					syncTime = Uptime();
					logger.Info("New game time from server: {0}", text);
				}
				else
				{
					logger.Error("Unable to parse server time '{0}'", text);
				}
			}
			else
			{
				logger.Error("Unable to set server time; using device time.");
			}
		}

		public bool WithinRange(int a, int b)
		{
			if (b < a)
			{
				int num = a;
				a = b;
				b = num;
			}
			int num2 = CurrentTime();
			return a <= num2 && num2 <= b;
		}

		public float RealtimeSinceStartup()
		{
			return (float)(DateTime.Now - startTime).TotalSeconds;
		}

		public bool WithinRange(IUTCRangeable rangeable, bool eternal = false)
		{
			if (eternal && rangeable.UTCStartDate == 0 && rangeable.UTCEndDate == 0)
			{
				return true;
			}
			return WithinRange(rangeable.UTCStartDate, rangeable.UTCEndDate);
		}

		public override string ToString()
		{
			return string.Format("Server: {0} Sync: {1} App: {2}", serverTimestamp, syncTime, appTickCount);
		}
	}
}

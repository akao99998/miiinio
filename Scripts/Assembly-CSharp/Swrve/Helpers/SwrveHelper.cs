using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Swrve.Helpers
{
	public static class SwrveHelper
	{
		public static DateTime? Now = null;

		public static DateTime? UtcNow = null;

		private static MD5CryptoServiceProvider fakeReference = new MD5CryptoServiceProvider();

		private static Regex rgxNonAlphanumeric = new Regex("[^a-zA-Z0-9]");

		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime GetNow()
		{
			DateTime? now = Now;
			if (now.HasValue && Now.HasValue)
			{
				return Now.Value;
			}
			return DateTime.Now;
		}

		public static DateTime GetUtcNow()
		{
			DateTime? utcNow = UtcNow;
			if (utcNow.HasValue && UtcNow.HasValue)
			{
				return UtcNow.Value;
			}
			return DateTime.UtcNow;
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int num = list.Count;
			Random random = new Random();
			while (num > 1)
			{
				int index = random.Next(0, num) % num;
				num--;
				T value = list[index];
				list[index] = list[num];
				list[num] = value;
			}
		}

		public static byte[] MD5(string str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			return SwrveMD5Core.GetHash(bytes);
		}

		public static string ApplyMD5(string str)
		{
			byte[] array = MD5(str);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		public static bool CheckBase64(string str)
		{
			string text = str.Trim();
			return text.Length % 4 == 0 && Regex.IsMatch(text, "^[a-zA-Z0-9\\+/]*={0,3}$", RegexOptions.None);
		}

		public static string CreateHMACMD5(string data, string key)
		{
			if (fakeReference != null)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(data);
				byte[] bytes2 = Encoding.UTF8.GetBytes(key);
				using (HMACMD5 hMACMD = new HMACMD5(bytes2))
				{
					byte[] bytes3 = hMACMD.ComputeHash(bytes);
					return Encoding.UTF8.GetString(bytes3);
				}
			}
			return null;
		}

		public static long GetSeconds()
		{
			return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
		}

		public static long GetMilliseconds()
		{
			return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
		}

		public static string GetEventName(Dictionary<string, object> eventParameters)
		{
			string result = string.Empty;
			switch ((string)eventParameters["type"])
			{
			case "session_start":
				result = "Swrve.session.start";
				break;
			case "session_end":
				result = "Swrve.session.end";
				break;
			case "buy_in":
				result = "Swrve.buy_in";
				break;
			case "iap":
				result = "Swrve.iap";
				break;
			case "event":
				result = (string)eventParameters["name"];
				break;
			case "purchase":
				result = "Swrve.user_purchase";
				break;
			case "currency_given":
				result = "Swrve.currency_given";
				break;
			case "user":
				result = "Swrve.user_properties_changed";
				break;
			}
			return result;
		}

		public static string EpochToFormat(long epochTime, string format)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(epochTime).ToString(format);
		}

		public static string FilterNonAlphanumeric(string str)
		{
			return rgxNonAlphanumeric.Replace(str, string.Empty);
		}
	}
}

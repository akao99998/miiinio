using System;
using System.Collections.Generic;
using System.Text;
using Swrve.Helpers;
using Swrve.REST;
using SwrveMiniJSON;

namespace Swrve.Messaging
{
	public class SwrveQAUser
	{
		private const int ApiVersion = 1;

		private const long SessionInterval = 1000L;

		private const long TriggerInterval = 500L;

		private const long PushNotificationInterval = 1000L;

		private readonly SwrveSDK swrve;

		private readonly IRESTClient restClient;

		private readonly string loggingUrl;

		private long lastSessionRequestTime;

		private long lastTriggerRequestTime;

		private long lastPushNotificationRequestTime;

		public readonly bool ResetDevice;

		public readonly bool Logging;

		public SwrveQAUser(SwrveSDK swrve, Dictionary<string, object> jsonQa)
		{
			this.swrve = swrve;
			ResetDevice = MiniJsonHelper.GetBool(jsonQa, "reset_device_state", false);
			Logging = MiniJsonHelper.GetBool(jsonQa, "logging", false);
			if (Logging)
			{
				restClient = new RESTClient();
				loggingUrl = MiniJsonHelper.GetString(jsonQa, "logging_url", null);
			}
		}

		public void TalkSession(Dictionary<int, string> campaignsDownloaded)
		{
			try
			{
				if (CanMakeSessionRequest())
				{
					string endpoint = loggingUrl + "/talk/game/" + swrve.ApiKey + "/user/" + swrve.UserId + "/session";
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					IList<object> list = new List<object>();
					Dictionary<int, string>.Enumerator enumerator = campaignsDownloaded.GetEnumerator();
					while (enumerator.MoveNext())
					{
						int key = enumerator.Current.Key;
						string value = enumerator.Current.Value;
						Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
						dictionary2.Add("id", key);
						dictionary2.Add("reason", (value != null) ? value : string.Empty);
						dictionary2.Add("loaded", value == null);
						list.Add(dictionary2);
					}
					dictionary.Add("campaigns", list);
					Dictionary<string, string> deviceInfo = swrve.GetDeviceInfo();
					dictionary.Add("device", deviceInfo);
					MakeRequest(endpoint, dictionary);
				}
			}
			catch (Exception ex)
			{
				SwrveLog.LogError("QA request talk session failed: " + ex.ToString());
			}
		}

		public void UpdateDeviceInfo()
		{
			try
			{
				if (!CanMakeRequest())
				{
					return;
				}
				string endpoint = loggingUrl + "/talk/game/" + swrve.ApiKey + "/user/" + swrve.UserId + "/device_info";
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				Dictionary<string, string> deviceInfo = swrve.GetDeviceInfo();
				foreach (string key in deviceInfo.Keys)
				{
					dictionary.Add(key, deviceInfo[key]);
				}
				MakeRequest(endpoint, dictionary);
			}
			catch (Exception ex)
			{
				SwrveLog.LogError("QA request talk device info update failed: " + ex.ToString());
			}
		}

		private void MakeRequest(string endpoint, Dictionary<string, object> json)
		{
			json.Add("version", 1);
			json.Add("client_time", DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"));
			string s = Json.Serialize(json);
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Content-Type", "application/json; charset=utf-8");
			dictionary.Add("Content-Length", bytes.Length.ToString());
			Dictionary<string, string> headers = dictionary;
			swrve.Container.StartCoroutine(restClient.Post(endpoint, bytes, headers, RestListener));
		}

		public void TriggerFailure(string eventName, string globalReason)
		{
			try
			{
				if (CanMakeTriggerRequest())
				{
					string endpoint = loggingUrl + "/talk/game/" + swrve.ApiKey + "/user/" + swrve.UserId + "/trigger";
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("trigger_name", eventName);
					dictionary.Add("displayed", false);
					dictionary.Add("reason", globalReason);
					dictionary.Add("campaigns", new List<object>());
					MakeRequest(endpoint, dictionary);
				}
			}
			catch (Exception ex)
			{
				SwrveLog.LogError("QA request talk session failed: " + ex.ToString());
			}
		}

		public void Trigger(string eventName, SwrveMessage messageShown, Dictionary<int, string> campaignReasons, Dictionary<int, int> campaignMessages)
		{
			try
			{
				if (!CanMakeTriggerRequest())
				{
					return;
				}
				string endpoint = loggingUrl + "/talk/game/" + swrve.ApiKey + "/user/" + swrve.UserId + "/trigger";
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("trigger_name", eventName);
				dictionary.Add("displayed", messageShown != null);
				dictionary.Add("reason", (messageShown != null) ? string.Empty : "The loaded campaigns returned no message");
				IList<object> list = new List<object>();
				Dictionary<int, string>.Enumerator enumerator = campaignReasons.GetEnumerator();
				while (enumerator.MoveNext())
				{
					int key = enumerator.Current.Key;
					string value = enumerator.Current.Value;
					int? num = null;
					if (campaignMessages.ContainsKey(key))
					{
						num = campaignMessages[key];
					}
					Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
					dictionary2.Add("id", key);
					dictionary2.Add("displayed", false);
					dictionary2.Add("message_id", num ?? new int?(-1));
					dictionary2.Add("reason", (value != null) ? value : string.Empty);
					list.Add(dictionary2);
				}
				if (messageShown != null)
				{
					Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
					dictionary3.Add("id", messageShown.Campaign.Id);
					dictionary3.Add("displayed", true);
					dictionary3.Add("message_id", messageShown.Id);
					dictionary3.Add("reason", string.Empty);
					list.Add(dictionary3);
				}
				dictionary.Add("campaigns", list);
				MakeRequest(endpoint, dictionary);
			}
			catch (Exception ex)
			{
				SwrveLog.LogError("QA request talk session failed: " + ex.ToString());
			}
		}

		private bool CanMakeRequest()
		{
			return swrve != null && Logging;
		}

		private bool CanMakeSessionRequest()
		{
			if (CanMakeRequest())
			{
				long milliseconds = SwrveHelper.GetMilliseconds();
				if (lastSessionRequestTime == 0L || milliseconds - lastSessionRequestTime > 1000)
				{
					lastSessionRequestTime = milliseconds;
					return true;
				}
			}
			return false;
		}

		private bool CanMakeTriggerRequest()
		{
			if (CanMakeRequest())
			{
				long milliseconds = SwrveHelper.GetMilliseconds();
				if (lastTriggerRequestTime == 0L || milliseconds - lastTriggerRequestTime > 500)
				{
					lastTriggerRequestTime = milliseconds;
					return true;
				}
			}
			return false;
		}

		private bool CanMakePushNotificationRequest()
		{
			if (swrve != null && Logging)
			{
				long milliseconds = SwrveHelper.GetMilliseconds();
				if (lastPushNotificationRequestTime == 0L || milliseconds - lastPushNotificationRequestTime > 1000)
				{
					lastPushNotificationRequestTime = milliseconds;
					return true;
				}
			}
			return false;
		}

		private void RestListener(RESTResponse response)
		{
			if (response.Error != 0)
			{
				SwrveLog.LogError("QA request to failed with error code " + response.Error.ToString() + ": " + response.Body);
			}
		}
	}
}

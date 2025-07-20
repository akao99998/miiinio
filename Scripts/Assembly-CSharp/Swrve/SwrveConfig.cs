using System;
using System.Collections.Generic;
using System.Globalization;
using Swrve.Messaging;
using UnityEngine;

namespace Swrve
{
	[Serializable]
	public class SwrveConfig
	{
		public const string DefaultEventsServer = "http://api.swrve.com";

		public const string DefaultContentServer = "http://content.swrve.com";

		public string UserId;

		public string AppVersion;

		public string AppStore = "google";

		public string Language;

		public string DefaultLanguage = "en";

		public bool TalkEnabled = true;

		public bool AutoDownloadCampaignsAndResources = true;

		public SwrveOrientation Orientation = SwrveOrientation.Both;

		public string EventsServer = "http://api.swrve.com";

		public bool UseHttpsForEventsServer;

		public string ContentServer = "http://content.swrve.com";

		public bool UseHttpsForContentServer;

		public bool AutomaticSessionManagement = true;

		public int NewSessionInterval = 30;

		public int MaxBufferChars = 262144;

		public bool SendEventsIfBufferTooLarge = true;

		public bool StoreDataInPlayerPrefs;

		public bool PushNotificationEnabled;

		public HashSet<string> PushNotificationEvents = new HashSet<string> { "Swrve.session.start" };

		public string GCMSenderId;

		public string GCMPushNotificationTitle = "#Your App Title";

		public float AutoShowMessagesMaxDelay = 5f;

		public Color? DefaultBackgroundColor;

		public CultureInfo Culture
		{
			set
			{
				Language = value.Name;
			}
		}

		public void CalculateEndpoints(int appId)
		{
			if (EventsServer == "http://api.swrve.com")
			{
				EventsServer = CalculateEndpoint(UseHttpsForEventsServer, appId, "api.swrve.com");
			}
			if (ContentServer == "http://content.swrve.com")
			{
				ContentServer = CalculateEndpoint(UseHttpsForContentServer, appId, "content.swrve.com");
			}
		}

		private static string HttpSchema(bool useHttps)
		{
			return (!useHttps) ? "http" : "https";
		}

		private static string CalculateEndpoint(bool useHttps, int appId, string suffix)
		{
			return HttpSchema(useHttps) + "://" + appId + "." + suffix;
		}
	}
}

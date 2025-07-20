using System;
using System.Collections.Generic;
using Swrve.Helpers;

namespace Swrve.Messaging
{
	public class SwrveCampaign
	{
		protected const string WaitTimeFormat = "HH\\:mm\\:ss zzz";

		protected const int DefaultDelayFirstMessage = 180;

		protected const long DefaultMaxShows = 99999L;

		protected const int DefaultMinDelay = 60;

		protected readonly Random rnd = new Random();

		public int Id;

		public List<SwrveMessage> Messages;

		public HashSet<string> Triggers;

		public DateTime StartDate;

		public DateTime EndDate;

		public int Impressions;

		public int Next;

		public bool RandomOrder;

		protected readonly DateTime swrveInitialisedTime;

		protected readonly string assetPath;

		protected DateTime showMessagesAfterLaunch;

		protected DateTime showMessagesAfterDelay;

		protected int minDelayBetweenMessage;

		protected int delayFirstMessage = 180;

		protected int maxImpressions;

		private SwrveCampaign(DateTime initialisedTime, string assetPath)
		{
			swrveInitialisedTime = initialisedTime;
			this.assetPath = assetPath;
			Messages = new List<SwrveMessage>();
			Triggers = new HashSet<string>();
			minDelayBetweenMessage = 60;
			showMessagesAfterLaunch = swrveInitialisedTime + TimeSpan.FromSeconds(180.0);
		}

		public SwrveMessage GetMessageForEvent(string triggerEvent, Dictionary<int, string> campaignReasons)
		{
			DateTime utcNow = SwrveHelper.GetUtcNow();
			DateTime now = SwrveHelper.GetNow();
			int count = Messages.Count;
			if (!HasMessageForEvent(triggerEvent))
			{
				SwrveLog.Log("There is no trigger in " + Id + " that matches " + triggerEvent);
				return null;
			}
			if (count == 0)
			{
				LogAndAddReason(campaignReasons, "No messages in campaign " + Id);
				return null;
			}
			if (StartDate > utcNow)
			{
				LogAndAddReason(campaignReasons, "Campaign " + Id + " has not started yet");
				return null;
			}
			if (EndDate < utcNow)
			{
				LogAndAddReason(campaignReasons, "Campaign" + Id + " has finished");
				return null;
			}
			if (Impressions >= maxImpressions)
			{
				LogAndAddReason(campaignReasons, "{Campaign throttle limit} Campaign " + Id + " has been shown " + maxImpressions + " times already");
				return null;
			}
			if (!string.Equals(triggerEvent, "Swrve.Messages.showAtSessionStart", StringComparison.OrdinalIgnoreCase) && IsTooSoonToShowMessageAfterLaunch(now))
			{
				LogAndAddReason(campaignReasons, "{Campaign throttle limit} Too soon after launch. Wait until " + showMessagesAfterLaunch.ToString("HH\\:mm\\:ss zzz"));
				return null;
			}
			if (IsTooSoonToShowMessageAfterDelay(now))
			{
				LogAndAddReason(campaignReasons, "{Campaign throttle limit} Too soon after last message. Wait until " + showMessagesAfterDelay.ToString("HH\\:mm\\:ss zzz"));
				return null;
			}
			SwrveLog.Log(triggerEvent + " matches a trigger in " + Id);
			return GetNextMessage(count, campaignReasons);
		}

		protected void LogAndAddReason(Dictionary<int, string> campaignReasons, string reason)
		{
			if (campaignReasons != null)
			{
				campaignReasons.Add(Id, reason);
			}
			SwrveLog.Log(reason);
		}

		public bool HasMessageForEvent(string eventName)
		{
			string item = eventName.ToLower();
			return Triggers != null && Triggers.Contains(item);
		}

		public SwrveMessage GetMessageForId(int id)
		{
			foreach (SwrveMessage message in Messages)
			{
				if (message.Id == id)
				{
					return message;
				}
			}
			return null;
		}

		protected SwrveMessage GetNextMessage(int messagesCount, Dictionary<int, string> campaignReasons)
		{
			if (RandomOrder)
			{
				List<SwrveMessage> list = new List<SwrveMessage>(Messages);
				list.Shuffle();
				foreach (SwrveMessage item in list)
				{
					if (item.isDownloaded(assetPath))
					{
						return item;
					}
				}
			}
			else if (Next < messagesCount)
			{
				SwrveMessage swrveMessage = Messages[Next];
				if (swrveMessage.isDownloaded(assetPath))
				{
					return swrveMessage;
				}
			}
			LogAndAddReason(campaignReasons, "Campaign " + Id + " hasn't finished downloading.");
			return null;
		}

		protected void AddMessage(SwrveMessage message)
		{
			Messages.Add(message);
		}

		public static SwrveCampaign LoadFromJSON(SwrveSDK sdk, Dictionary<string, object> campaignData, DateTime initialisedTime, string assetPath)
		{
			SwrveCampaign swrveCampaign = new SwrveCampaign(initialisedTime, assetPath);
			swrveCampaign.Id = MiniJsonHelper.GetInt(campaignData, "id");
			AssignCampaignTriggers(swrveCampaign, campaignData);
			AssignCampaignRules(swrveCampaign, campaignData);
			AssignCampaignDates(swrveCampaign, campaignData);
			IList<object> list = (IList<object>)campaignData["messages"];
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				Dictionary<string, object> messageData = (Dictionary<string, object>)list[i];
				SwrveMessage swrveMessage = SwrveMessage.LoadFromJSON(sdk, swrveCampaign, messageData);
				if (swrveMessage.Formats.Count > 0)
				{
					swrveCampaign.AddMessage(swrveMessage);
				}
			}
			return swrveCampaign;
		}

		public List<string> ListOfAssets()
		{
			List<string> list = new List<string>();
			foreach (SwrveMessage message in Messages)
			{
				list.AddRange(message.ListOfAssets());
			}
			return list;
		}

		protected static void AssignCampaignTriggers(SwrveCampaign campaign, Dictionary<string, object> campaignData)
		{
			IList<object> list = (IList<object>)campaignData["triggers"];
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				string text = (string)list[i];
				campaign.Triggers.Add(text.ToLower());
			}
		}

		protected static void AssignCampaignRules(SwrveCampaign campaign, Dictionary<string, object> campaignData)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)campaignData["rules"];
			campaign.RandomOrder = ((string)dictionary["display_order"]).Equals("random");
			if (dictionary.ContainsKey("dismiss_after_views"))
			{
				int @int = MiniJsonHelper.GetInt(dictionary, "dismiss_after_views");
				campaign.maxImpressions = @int;
			}
			if (dictionary.ContainsKey("delay_first_message"))
			{
				campaign.delayFirstMessage = MiniJsonHelper.GetInt(dictionary, "delay_first_message");
				campaign.showMessagesAfterLaunch = campaign.swrveInitialisedTime + TimeSpan.FromSeconds(campaign.delayFirstMessage);
			}
			if (dictionary.ContainsKey("min_delay_between_messages"))
			{
				int int2 = MiniJsonHelper.GetInt(dictionary, "min_delay_between_messages");
				campaign.minDelayBetweenMessage = int2;
			}
		}

		protected static void AssignCampaignDates(SwrveCampaign campaign, Dictionary<string, object> campaignData)
		{
			DateTime unixEpoch = SwrveHelper.UnixEpoch;
			campaign.StartDate = unixEpoch.AddMilliseconds(MiniJsonHelper.GetLong(campaignData, "start_date"));
			campaign.EndDate = unixEpoch.AddMilliseconds(MiniJsonHelper.GetLong(campaignData, "end_date"));
		}

		public void IncrementImpressions()
		{
			Impressions++;
		}

		protected bool IsTooSoonToShowMessageAfterLaunch(DateTime now)
		{
			return now < showMessagesAfterLaunch;
		}

		protected bool IsTooSoonToShowMessageAfterDelay(DateTime now)
		{
			return now < showMessagesAfterDelay;
		}

		protected void SetMessageMinDelayThrottle()
		{
			showMessagesAfterDelay = SwrveHelper.GetNow() + TimeSpan.FromSeconds(minDelayBetweenMessage);
		}

		public void MessageWasShownToUser(SwrveMessageFormat messageFormat)
		{
			IncrementImpressions();
			if (Messages.Count > 0)
			{
				SetMessageMinDelayThrottle();
				if (!RandomOrder)
				{
					int num = (Next = (Next + 1) % Messages.Count);
					SwrveLog.Log("Round Robin: Next message in campaign " + Id + " is " + num);
				}
				else
				{
					SwrveLog.Log("Next message in campaign " + Id + " is random");
				}
			}
		}

		public void MessageDismissed()
		{
			SetMessageMinDelayThrottle();
		}
	}
}

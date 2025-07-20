using System.Collections.Generic;
using Swrve.Helpers;

namespace Swrve.Messaging
{
	public class SwrveMessage
	{
		public int Id;

		public string Name;

		public int Priority = 9999;

		public SwrveCampaign Campaign;

		public List<SwrveMessageFormat> Formats;

		public Point Position = new Point(0, 0);

		public Point TargetPosition = new Point(0, 0);

		public float BackgroundAlpha = 1f;

		public float AnimationScale = 1f;

		private SwrveMessage(SwrveCampaign campaign)
		{
			Campaign = campaign;
			Formats = new List<SwrveMessageFormat>();
		}

		public SwrveMessageFormat GetFormat(SwrveOrientation orientation)
		{
			IEnumerator<SwrveMessageFormat> enumerator = Formats.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Orientation == orientation)
				{
					return enumerator.Current;
				}
			}
			return null;
		}

		public static SwrveMessage LoadFromJSON(SwrveSDK sdk, SwrveCampaign campaign, Dictionary<string, object> messageData)
		{
			SwrveMessage swrveMessage = new SwrveMessage(campaign);
			swrveMessage.Id = MiniJsonHelper.GetInt(messageData, "id");
			swrveMessage.Name = (string)messageData["name"];
			if (messageData.ContainsKey("priority"))
			{
				swrveMessage.Priority = MiniJsonHelper.GetInt(messageData, "priority");
			}
			Dictionary<string, object> dictionary = (Dictionary<string, object>)messageData["template"];
			IList<object> list = (List<object>)dictionary["formats"];
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				Dictionary<string, object> messageFormatData = (Dictionary<string, object>)list[i];
				SwrveMessageFormat item = SwrveMessageFormat.LoadFromJSON(sdk, swrveMessage, messageFormatData);
				swrveMessage.Formats.Add(item);
			}
			return swrveMessage;
		}

		public bool SupportsOrientation(SwrveOrientation orientation)
		{
			return GetFormat(orientation) != null;
		}

		public List<string> ListOfAssets()
		{
			List<string> list = new List<string>();
			foreach (SwrveMessageFormat format in Formats)
			{
				foreach (SwrveImage image in format.Images)
				{
					if (!string.IsNullOrEmpty(image.File))
					{
						list.Add(image.File);
					}
				}
				foreach (SwrveButton button in format.Buttons)
				{
					if (!string.IsNullOrEmpty(button.Image))
					{
						list.Add(button.Image);
					}
				}
			}
			return list;
		}

		public bool isDownloaded(string assetPath)
		{
			List<string> list = ListOfAssets();
			foreach (string item in list)
			{
				if (!CrossPlatformFile.Exists(assetPath + "/" + item))
				{
					return false;
				}
			}
			return true;
		}
	}
}

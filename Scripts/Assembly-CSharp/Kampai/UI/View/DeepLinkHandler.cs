using System;
using System.Collections;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class DeepLinkHandler : MonoBehaviour
	{
		private bool waitingToProcessLink;

		public IKampaiLogger logger { get; set; }

		public MoveBuildMenuSignal moveBuildMenuSignal { get; set; }

		public ShowMTXStoreSignal showMTXStoreSignal { get; set; }

		public CloneUserFromEnvSignal cloneUserFromEnvSignal { get; set; }

		private void Awake()
		{
			waitingToProcessLink = true;
			StartCoroutine(WaitToProcessLink());
		}

		public virtual void OnDeepLink(string uriString)
		{
			if (!waitingToProcessLink)
			{
				waitingToProcessLink = true;
				StartCoroutine(WaitToProcessLink());
			}
		}

		private IEnumerator WaitToProcessLink()
		{
			yield return new WaitForSeconds(0.5f);
			ProcessDeepLink();
			waitingToProcessLink = false;
		}

		private void RemoveLinkFromPrefs()
		{
			PlayerPrefs.DeleteKey("DeepLink");
			PlayerPrefs.Save();
		}

		internal void ProcessDeepLink()
		{
			string @string = PlayerPrefs.GetString("DeepLink");
			if (@string.Length == 0)
			{
				RemoveLinkFromPrefs();
				return;
			}
			Uri uri = new Uri(@string);
			logger.Debug("uri.Host: {0}", uri.Host);
			if (uri.Host != "deeplink")
			{
				logger.Error("Not a deeplink url: {0}", @string);
				RemoveLinkFromPrefs();
				return;
			}
			string absolutePath = uri.AbsolutePath;
			string[] array = absolutePath.Split('/');
			if (array.Length < 3)
			{
				logger.Error("Incorrect deeplink url: {0}", @string);
				RemoveLinkFromPrefs();
				return;
			}
			string text = array[1];
			logger.Debug("action = {0}", text);
			switch (text)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					if (GameConstants.StaticConfig.DEBUG_ENABLED && array.Length == 4)
					{
						cloneUserFromEnvSignal.Dispatch(array[2], Convert.ToInt64(array[3]));
						break;
					}
					logger.Error("Incorrect deeplink url: {0}", @string);
				}
				else
				{
					logger.Error("Unsupported action: {0}", text);
				}
				break;
			}
			case "view":
			{
				string text2 = array[2];
				logger.Debug("target = {0}", text2);
				switch (text2)
				{
				case "build_menu":
					moveBuildMenuSignal.Dispatch(true);
					break;
				case "grind_store":
					showMTXStoreSignal.Dispatch(new Tuple<int, int>(800001, 0));
					break;
				case "premium_store":
					showMTXStoreSignal.Dispatch(new Tuple<int, int>(800002, 0));
					break;
				default:
					logger.Error("Unsupported target: {0}", text2);
					break;
				}
				break;
			}
			}
			RemoveLinkFromPrefs();
		}
	}
}

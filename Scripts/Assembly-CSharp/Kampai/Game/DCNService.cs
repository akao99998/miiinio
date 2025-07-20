using System;
using System.Collections.Generic;
using System.Text;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Splash;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class DCNService : IDCNService
	{
		public const string DCNPersistenceKey = "DCNStore";

		public const string DCNPersistenceDoNotShowKey = "DCNStoreDoNotShow";

		public const int MaxSeenMemory = 10;

		private const char DELIMITER = ',';

		public IKampaiLogger logger = LogManager.GetClassLogger("DCNService") as IKampaiLogger;

		private DCNModel dcnModel = new DCNModel();

		private IList<Func<IRequest>> requests = new List<Func<IRequest>>();

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public DCNTokenSignal dcnTokenSignal { get; set; }

		[Inject]
		public DCNEventSignal eventSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		public void Perform(Func<IRequest> request, bool isTokenRequest = false)
		{
			if (!coppaService.Restricted())
			{
				if ((dcnModel.Token == null || !dcnModel.Token.IsValid()) && !isTokenRequest)
				{
					requests.Add(request);
					dcnTokenSignal.Dispatch();
					return;
				}
				IRequest request2 = request();
				logger.Info("DCN Request {0}", request2.Uri);
				downloadService.Perform(request2);
			}
		}

		public void SetToken(DCNToken token)
		{
			logger.Info("DCN Token {0}", token.Token);
			dcnModel.Token = token;
			if (requests.Count > 0)
			{
				Perform(requests[0]);
				requests.RemoveAt(0);
			}
		}

		public string GetToken()
		{
			if (dcnModel == null)
			{
				return string.Empty;
			}
			if (dcnModel.Token == null)
			{
				return string.Empty;
			}
			return dcnModel.Token.Token;
		}

		private List<int> SeenContentIds()
		{
			List<int> list = new List<int>();
			string data = localPersistanceService.GetData("DCNStore");
			if (!string.IsNullOrEmpty(data))
			{
				string[] array = data.Split(',');
				string[] array2 = array;
				foreach (string text in array2)
				{
					try
					{
						int item = Convert.ToInt32(text);
						list.Add(item);
					}
					catch (Exception ex)
					{
						logger.Error("DCN Bad Content ID {0} {1}", text, ex.Message);
					}
				}
			}
			return list;
		}

		public bool HasSeenFeaturedContent(int featuredContentId)
		{
			return SeenContentIds().Contains(featuredContentId);
		}

		public void MarkFeaturedContentAsSeen(int featuredContentId)
		{
			List<int> list = SeenContentIds();
			list.Add(featuredContentId);
			while (list.Count > 10)
			{
				list.RemoveAt(0);
			}
			StringBuilder stringBuilder = new StringBuilder();
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				stringBuilder.Append(list[i].ToString());
				if (i < count - 1)
				{
					stringBuilder.Append(',');
				}
			}
			localPersistanceService.PutData("DCNStore", stringBuilder.ToString());
		}

		public bool SetFeaturedContent(int featuredContentId, string htmlUrl)
		{
			logger.Info("DCN featured content ID={0} URL={1}", featuredContentId, htmlUrl);
			if (HasSeenFeaturedContent(featuredContentId))
			{
				logger.Warning("DCN Ignoring seen content {0}", featuredContentId);
				return false;
			}
			dcnModel.FeaturedContentId = featuredContentId;
			dcnModel.FeaturedUrl = htmlUrl;
			return true;
		}

		public int GetFeaturedContentId()
		{
			return dcnModel.FeaturedContentId;
		}

		public void OpenFeaturedContent(bool open)
		{
			if (!HasFeaturedContent())
			{
				logger.Error("Unable to show DCN content [no url]");
				return;
			}
			if (open)
			{
				MarkFeaturedContentAsSeen(dcnModel.FeaturedContentId);
			}
			string launchURL = GetLaunchURL();
			if (open)
			{
				logger.Info("Launching DCN Content {0}", launchURL);
				Application.OpenURL(launchURL);
				eventSignal.Dispatch();
			}
			else
			{
				logger.Info("Declining DCN Content {0}", launchURL);
			}
		}

		public string GetLaunchURL()
		{
			string value = WWW.EscapeURL("minions:\\dcn");
			StringBuilder stringBuilder = new StringBuilder(dcnModel.FeaturedUrl).Append("&token=").Append(GetToken()).Append("&return_name=")
				.Append("Minions")
				.Append("&return_url=")
				.Append(value);
			return stringBuilder.ToString();
		}

		public bool HasFeaturedContent()
		{
			return !string.IsNullOrEmpty(dcnModel.FeaturedUrl);
		}
	}
}

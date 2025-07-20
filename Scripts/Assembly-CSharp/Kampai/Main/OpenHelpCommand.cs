using System.Collections.Generic;
using System.Text;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class OpenHelpCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("OpenHelpCommand") as IKampaiLogger;

		private string platform;

		[Inject]
		public HelpType helpType { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ILocalizationService loc { get; set; }

		[Inject]
		public ILocalPersistanceService LocalPersistService { get; set; }

		[Inject]
		public IEncryptionService encryptionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public IClientVersion clientVersion { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		public override void Execute()
		{
			string text = BuildUrl();
			logger.Info(text);
			userSessionService.OpenURL(text);
		}

		private string GetWWCELocaleCode()
		{
			string result = loc.GetLanguage();
			if (loc.GetLanguage().Equals("pt"))
			{
				result = "br";
			}
			return result;
		}

		public string BuildUrl()
		{
			string helpPlatform = getHelpPlatform();
			if (helpType == HelpType.ONLINE_HELP)
			{
				Dictionary<string, string> value = preparePlayLoad();
				string text = JsonConvert.SerializeObject(value);
				logger.Info("Tesla tptk payload :" + text);
				string text2 = WWW.EscapeURL(TeslaActivate.Encrypt(text, GameConstants.StaticConfig.WWCE_SECRET));
				return string.Format(GameConstants.StaticConfig.WWCE_URL, GetWWCELocaleCode(), GameConstants.StaticConfig.WWCE_GAME_NAME, GameConstants.StaticConfig.WWCE_GAME_NAME, helpPlatform, GameConstants.StaticConfig.WWCE_GAME_NAME, text2);
			}
			return string.Format(GameConstants.StaticConfig.WWCE_CONTACTUS_URL, GetWWCELocaleCode(), GameConstants.StaticConfig.WWCE_GAME_NAME, GameConstants.StaticConfig.WWCE_GAME_NAME, helpPlatform);
		}

		public string getHelpPlatform()
		{
			if (platform == null)
			{
				platform = ((!DeviceCapabilities.IsTablet()) ? "android-phone" : "android-tablet");
			}
			return platform;
		}

		private string getDefinitionVariants()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(configurationsService.GetDefinitionVariants()).Replace("_", ",");
			return stringBuilder.ToString();
		}

		private Dictionary<string, string> preparePlayLoad()
		{
			string plainText = LocalPersistService.GetData("AnonymousID");
			encryptionService.TryDecrypt(plainText, "Kampai!", out plainText);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("platform", getHelpPlatform());
			UserSession userSession = userSessionService.UserSession;
			dictionary.Add("internal", LocalPersistService.GetData("UserID"));
			dictionary.Add("anonymous", plainText);
			dictionary.Add("synergy", userSession.SynergyID);
			dictionary.Add("Level", playerService.GetQuantity(StaticItem.LEVEL_ID).ToString());
			dictionary.Add("Grind Currency", playerService.GetQuantity(StaticItem.GRIND_CURRENCY_ID).ToString());
			dictionary.Add("Preminum Currency", playerService.GetQuantity(StaticItem.PREMIUM_CURRENCY_ID).ToString());
			dictionary.Add("Game Play Duration", playerDurationService.GameplaySecondsSinceLevelUp.ToString());
			dictionary.Add("Build", clientVersion.GetClientVersion());
			dictionary.Add("Age", coppaService.GetAge().ToString());
			dictionary.Add("Definition Variants", getDefinitionVariants());
			dictionary.Add("spend", playerService.GetQuantity(StaticItem.TRANSACTIONS_LIFETIME_COUNT_ID).ToString());
			AddSocialIdentitiesToPayload(dictionary);
			return dictionary;
		}

		private void AddSocialIdentitiesToPayload(Dictionary<string, string> payload)
		{
			UserSession userSession = userSessionService.UserSession;
			IList<UserIdentity> socialIdentities = userSession.SocialIdentities;
			if (socialIdentities == null)
			{
				return;
			}
			foreach (UserIdentity socialIdentity in userSession.SocialIdentities)
			{
				string externalID = socialIdentity.ExternalID;
				if (!string.IsNullOrEmpty(externalID))
				{
					payload[socialIdentity.Type.ToString()] = externalID;
				}
			}
		}
	}
}

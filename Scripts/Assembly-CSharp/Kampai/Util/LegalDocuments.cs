using Kampai.Game;
using Kampai.Main;
using UnityEngine;

namespace Kampai.Util
{
	public static class LegalDocuments
	{
		public enum LegalType
		{
			EULA = 0,
			TOS = 1,
			PRIVACY = 2
		}

		public static void TermsOfServiceClicked(ILocalizationService loc, PlayGlobalSoundFXSignal soundFXSignal, IKampaiLogger logger, IDefinitionService defService)
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			Application.OpenURL(GetLocalizedURL(loc, LegalType.TOS, logger, defService));
		}

		public static void PrivacyPolicyClicked(ILocalizationService loc, PlayGlobalSoundFXSignal soundFXSignal, IKampaiLogger logger, IDefinitionService defService)
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			Application.OpenURL(GetLocalizedURL(loc, LegalType.PRIVACY, logger, defService));
		}

		public static void EulaClicked(ILocalizationService loc, PlayGlobalSoundFXSignal soundFXSignal, IKampaiLogger logger, IDefinitionService defService)
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			Application.OpenURL(GetLocalizedURL(loc, LegalType.EULA, logger, defService));
		}

		public static string GetLocalizedURL(ILocalizationService loc, LegalType legalType, IKampaiLogger logger, IDefinitionService defService)
		{
			string text = loc.GetLanguage();
			if (text == "zh")
			{
				text = ((!(loc.GetLanguageKey() == "ZH-CN")) ? "tc" : "sc");
			}
			else if (text == "pt" && loc.GetLanguageKey() == "PT-BR")
			{
				text = "br";
			}
			string text2 = "mobileeula";
			string text3 = "PC";
			switch (legalType)
			{
			case LegalType.EULA:
			{
				text2 = "mobileeula";
				text3 = ((Application.platform != RuntimePlatform.IPhonePlayer) ? "GM" : "OTHER");
				string legalURL = defService.GetLegalURL(LegalType.EULA, text);
				return string.Format(legalURL, text2, text3);
			}
			case LegalType.TOS:
				return defService.GetLegalURL(LegalType.TOS, text);
			case LegalType.PRIVACY:
				return defService.GetLegalURL(LegalType.PRIVACY, text);
			default:
				logger.Error("Supported LegalType must be specified");
				return string.Empty;
			}
		}
	}
}

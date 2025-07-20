using System;
using System.IO;
using Kampai.Main;
using Kampai.Util;

public static class HindsightUtil
{
	public static string GetUri(HindsightCampaignDefinition definition, string languageKey)
	{
		if (definition.URI.ContainsKey(languageKey))
		{
			return definition.URI[languageKey].ToString();
		}
		if (definition.URI.ContainsKey("default"))
		{
			return definition.URI["default"].ToString();
		}
		return string.Empty;
	}

	public static string GetContentUri(HindsightCampaignDefinition definition, string languageKey)
	{
		if (definition.Content.ContainsKey(languageKey))
		{
			return definition.Content[languageKey].ToString();
		}
		if (definition.Content.ContainsKey("default"))
		{
			return definition.Content["default"].ToString();
		}
		return string.Empty;
	}

	public static string GetContentCachePath(HindsightCampaignDefinition definition, string languageKey)
	{
		string contentUri = GetContentUri(definition, languageKey);
		if (string.IsNullOrEmpty(contentUri))
		{
			return string.Empty;
		}
		string extension = Path.GetExtension(contentUri);
		if (string.IsNullOrEmpty(extension) || !extension.Contains("."))
		{
			return string.Empty;
		}
		return string.Format("{0}{1}_{2}{3}", GameConstants.IMAGE_PATH, definition.ID, languageKey, extension);
	}

	public static HindsightCampaign.Scope GetScope(HindsightCampaignDefinition definition)
	{
		return (HindsightCampaign.Scope)(int)Enum.Parse(typeof(HindsightCampaign.Scope), definition.Scope);
	}

	public static bool ValidPlatform(HindsightCampaignDefinition definition)
	{
		HindsightCampaign.Platform platform = (HindsightCampaign.Platform)(int)Enum.Parse(typeof(HindsightCampaign.Platform), definition.Platform);
		if (platform == HindsightCampaign.Platform.all)
		{
			return true;
		}
		return platform == HindsightCampaign.Platform.android;
	}
}

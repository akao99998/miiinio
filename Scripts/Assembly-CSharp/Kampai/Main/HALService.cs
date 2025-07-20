using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Main
{
	public class HALService : ILocalizationService
	{
		private const char OPEN_KEY_DELIM = '{';

		private const char CLOSE_KEY_DELIM = '}';

		private const char VERSION_DELIM = '$';

		private const string KEY_NOT_FOUND = "KEY NOT FOUND";

		public IKampaiLogger logger = LogManager.GetClassLogger("HALService") as IKampaiLogger;

		private Dictionary<string, ILocalString> localStringDict;

		private string jsonPath;

		private bool isLanguageSupported = true;

		private string language;

		private string country;

		private CultureInfo m_cultureInfo;

		public CultureInfo CultureInfo
		{
			get
			{
				return m_cultureInfo;
			}
		}

		public void Initialize(string langCode)
		{
			jsonPath = GetResourcePath(langCode);
			if (string.IsNullOrEmpty(jsonPath))
			{
				isLanguageSupported = false;
				jsonPath = "EN-US";
			}
			language = ExtractLanguageFromLocale(jsonPath);
			try
			{
				localStringDict = GetLocalizedDictionary(Native.GetStreamingTextAsset(string.Format("{0}{1}.json", "Loc_Text_Preinstalled/", jsonPath)));
			}
			catch (FileNotFoundException ex)
			{
				logger.Error("Error obtaining preinstalled localization file: {0}", ex.ToString());
				localStringDict = new Dictionary<string, ILocalString>();
			}
			foreach (KeyValuePair<string, ILocalString> item in localStringDict)
			{
				ParseLocalString(item.Value);
			}
		}

		public bool IsInitialized()
		{
			return localStringDict != null;
		}

		public void Update()
		{
			TextAsset textAsset = KampaiResources.Load<TextAsset>(jsonPath);
			if (textAsset == null)
			{
				logger.Error("Error obtaining full localization asset: {0}", jsonPath);
				return;
			}
			foreach (KeyValuePair<string, ILocalString> item in GetLocalizedDictionary(textAsset.ToString()))
			{
				ParseLocalString(item.Value);
				localStringDict[item.Key] = item.Value;
			}
		}

		public string GetLanguage()
		{
			return language;
		}

		public string GetCountry()
		{
			return country;
		}

		public bool IsLanguageSupported()
		{
			return isLanguageSupported;
		}

		public bool Contains(string key)
		{
			if (key == null || !localStringDict.ContainsKey(key))
			{
				return false;
			}
			return true;
		}

		public string GetString(string key, params object[] args)
		{
			if (key == null)
			{
				string text = string.Format("{0}: {1}", "KEY NOT FOUND", "null key");
				logger.Log(KampaiLogLevel.Warning, text);
				return text;
			}
			if (!localStringDict.ContainsKey(key))
			{
				string text2 = string.Format("{0}: {1}", "KEY NOT FOUND", key);
				logger.Log(KampaiLogLevel.Warning, text2);
				return text2;
			}
			LocalQuantityString localQuantityString = localStringDict[key] as LocalQuantityString;
			if (localQuantityString != null)
			{
				return localQuantityString.GetStringFormat(args);
			}
			LocalString localString = localStringDict[key] as LocalString;
			if (localString != null)
			{
				return localString.GetStringFormat(args);
			}
			string text3 = string.Format("{0}: {1}", "KEY NOT FOUND", key);
			logger.Log(KampaiLogLevel.Warning, text3);
			return text3;
		}

		public string GetStringUpper(string key, params object[] args)
		{
			return StringToUpper(GetString(key, args));
		}

		public string StringToUpper(string str)
		{
			if (m_cultureInfo != null)
			{
				return str.ToUpper(m_cultureInfo);
			}
			return str.ToUpper();
		}

		public string GetStringLower(string key, params object[] args)
		{
			return StringToLower(GetString(key, args));
		}

		public string StringToLower(string str)
		{
			if (m_cultureInfo != null)
			{
				return str.ToLower(m_cultureInfo);
			}
			return str.ToLower();
		}

		private JsonConverter GetStringConverter()
		{
			return new LocalStringConverter();
		}

		private void ParseLocalString(ILocalString iLocalString)
		{
			LocalString localString = iLocalString as LocalString;
			if (localString == null)
			{
				return;
			}
			string @string = localString.GetString();
			int num = @string.IndexOf('{', 0);
			while (num != -1)
			{
				int num2 = @string.IndexOf('}', num);
				string text = @string.Substring(num + 1, num2 - num - 1);
				num = @string.IndexOf('{', num2);
				if (localStringDict.ContainsKey(text))
				{
					LocalString localString2 = localStringDict[text] as LocalString;
					if (localString2 != null)
					{
						ParseLocalString(localStringDict[text]);
						string tag = string.Format("{0}{1}{2}", '{', text, '}');
						string string2 = localString2.GetString();
						localString.SetKeyValue(tag, string2);
					}
				}
			}
		}

		public string GetLanguageKey()
		{
			return jsonPath;
		}

		public static string GetResourcePath(string languageCode)
		{
			languageCode = languageCode.ToLower();
			string text = languageCode;
			if (languageCode.Contains("_"))
			{
				text = languageCode.Split('_')[0];
			}
			else if (languageCode.Contains("-"))
			{
				text = languageCode.Split('-')[0];
			}
			if (text.Equals("en"))
			{
				return "EN-US";
			}
			if (text.Equals("fr"))
			{
				return "FR-FR";
			}
			if (text.Equals("de"))
			{
				return "DE-DE";
			}
			if (text.Equals("es"))
			{
				return "ES-ES";
			}
			if (text.Equals("it"))
			{
				return "IT-IT";
			}
			if (text.Equals("pt"))
			{
				return "PT-BR";
			}
			if (text.Equals("nl"))
			{
				return "NL-NL";
			}
			if (text.Equals("ko"))
			{
				return "KO-KR";
			}
			if (text.Equals("ru"))
			{
				return "RU-RU";
			}
			if (text.Equals("ja"))
			{
				return "JA";
			}
			if (languageCode.Equals("zh-hans") || languageCode.Equals("zh_hans") || languageCode.Equals("zh_cn") || languageCode.Equals("zh-cn"))
			{
				return "ZH-CN";
			}
			if (languageCode.Equals("zh-hant") || languageCode.Equals("zh_hant") || languageCode.Equals("zh_tw") || languageCode.Equals("zh-tw") || languageCode.Equals("zh_hk") || languageCode.Equals("zh-hk"))
			{
				return "ZH-TW";
			}
			if (text.Equals("zh"))
			{
				return "ZH-CN";
			}
			if (text.Equals("tr"))
			{
				return "TR";
			}
			if (text.Equals("id") || languageCode.Equals("in_id"))
			{
				return "ID";
			}
			return string.Empty;
		}

		public static string ExtractLanguageFromLocale(string locale)
		{
			return ((!locale.Contains("-")) ? locale : locale.Substring(0, locale.IndexOf('-'))).ToLower();
		}

		private Dictionary<string, ILocalString> GetLocalizedDictionary(string jsonString)
		{
			Dictionary<string, ILocalString> dictionary = new Dictionary<string, ILocalString>();
			Dictionary<string, ILocalString> dictionary2 = new Dictionary<string, ILocalString>();
			try
			{
				dictionary = JsonConvert.DeserializeObject<Dictionary<string, ILocalString>>(jsonString, new JsonConverter[1] { GetStringConverter() });
			}
			catch (FileNotFoundException ex)
			{
				logger.Error("Error obtaining preinstalled localization file: {0}", ex.ToString());
			}
			List<string> list = new List<string>(dictionary.Keys);
			list.Sort();
			Version version = new Version(Native.BundleVersion);
			foreach (string item in list)
			{
				int num = item.LastIndexOf('$');
				string key = item;
				Version version2 = null;
				if (num > 0 && num < item.Length - 1)
				{
					key = item.Substring(0, num);
					string text = item.Substring(num + 1);
					if (text.IndexOf('.') == -1)
					{
						text += ".0";
					}
					version2 = new Version(text);
				}
				if (version2 == null || version2 <= version)
				{
					dictionary2[key] = dictionary[item];
				}
			}
			return dictionary2;
		}

		public void RetrieveCultureInfo(IResponse response)
		{
			IDictionary<string, string> headers = response.Headers;
			string headerValue = null;
			if (headers != null && CaseInsensitiveHeaderSearch("X-Kampai-Country", headers, out headerValue))
			{
				logger.Info("New country code from server: {0}", headerValue);
				SetCultureInfo(headerValue);
			}
		}

		private bool CaseInsensitiveHeaderSearch(string headerName, IDictionary<string, string> headers, out string headerValue)
		{
			string text = headerName.ToLower();
			foreach (KeyValuePair<string, string> header in headers)
			{
				if (text == header.Key.ToLower())
				{
					headerValue = header.Value;
					return true;
				}
			}
			headerValue = null;
			return false;
		}

		public void SetCultureInfo(string cultureInfoStr)
		{
			string deviceLanguage = Native.GetDeviceLanguage();
			try
			{
				country = cultureInfoStr;
				cultureInfoStr = (deviceLanguage.Contains("zh") ? "zh-CN" : ((!string.IsNullOrEmpty(cultureInfoStr) && !deviceLanguage.Contains("_") && !deviceLanguage.Contains("-")) ? string.Format("{0}-{1}", deviceLanguage, cultureInfoStr) : deviceLanguage.Replace('_', '-')));
				m_cultureInfo = CultureInfo.CreateSpecificCulture(cultureInfoStr);
			}
			catch (ArgumentException)
			{
				try
				{
					deviceLanguage = deviceLanguage.Replace('_', '-');
					m_cultureInfo = CultureInfo.CreateSpecificCulture(deviceLanguage);
				}
				catch (ArgumentException ex)
				{
					logger.Error("Could not create Culture Info from {0}, error: {1}", m_cultureInfo, ex);
					m_cultureInfo = CultureInfo.InvariantCulture;
				}
			}
		}
	}
}

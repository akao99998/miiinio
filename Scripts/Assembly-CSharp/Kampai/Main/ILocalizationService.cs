using System.Globalization;
using Ea.Sharkbite.HttpPlugin.Http.Api;

namespace Kampai.Main
{
	public interface ILocalizationService
	{
		CultureInfo CultureInfo { get; }

		void Initialize(string langCode);

		bool IsInitialized();

		void Update();

		string GetLanguage();

		string GetCountry();

		bool IsLanguageSupported();

		string GetString(string key, params object[] args);

		string GetStringUpper(string key, params object[] args);

		string GetStringLower(string key, params object[] args);

		string StringToUpper(string str);

		string StringToLower(string str);

		string GetLanguageKey();

		bool Contains(string key);

		void RetrieveCultureInfo(IResponse response);

		void SetCultureInfo(string cultureInfoStr);
	}
}

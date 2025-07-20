using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Splash
{
	public class SavedTipsModel
	{
		public const char delimeter = '\u001d';

		public IList<TipToShow> Tips = new List<TipToShow>();

		public string TipsLocale = string.Empty;

		public SavedTipsModel()
		{
			InitTips();
		}

		private void InitTips()
		{
			if (PlayerPrefs.HasKey("SavedLocTipsWithTimes"))
			{
				string @string = PlayerPrefs.GetString("SavedLocTipsWithTimes");
				string[] array = @string.Split('\u001d');
				for (int i = 0; i < array.Length; i += 2)
				{
					float result = 0f;
					float.TryParse(array[i + 1], out result);
					TipToShow item = new TipToShow(array[i], result);
					Tips.Add(item);
				}
			}
			if (PlayerPrefs.HasKey("LastSavedTipsLocale"))
			{
				TipsLocale = PlayerPrefs.GetString("LastSavedTipsLocale");
			}
		}

		public void SaveTipsToDevice(IList<TipToShow> tips, string locale)
		{
			string text = string.Empty;
			for (int i = 0; i < tips.Count; i++)
			{
				TipToShow tipToShow = tips[i];
				if (i > 0)
				{
					text += '\u001d';
				}
				text += tipToShow.Text;
				text += '\u001d';
				text += tipToShow.Time;
			}
			PlayerPrefs.SetString("SavedLocTipsWithTimes", text);
			PlayerPrefs.SetString("LastSavedTipsLocale", locale);
		}
	}
}

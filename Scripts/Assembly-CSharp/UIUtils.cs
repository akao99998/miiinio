using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

public static class UIUtils
{
	private const int BASE_SCREEN_HEIGHT = 640;

	private const int BASE_SCREEN_WIDTH = 960;

	private static float widthScale;

	private static float heightScale;

	public static float GetScreenScale()
	{
		float num = GetWidthScale();
		float num2 = GetHeightScale();
		return (!(num > num2)) ? num : num2;
	}

	public static void ScaleFonts(GameObject gameobject)
	{
		float screenScale = GetScreenScale();
		Text[] componentsInChildren = gameobject.GetComponentsInChildren<Text>();
		Text[] array = componentsInChildren;
		foreach (Text text in array)
		{
			text.fontSize = (int)((float)text.fontSize * screenScale);
		}
	}

	public static void MultiplyFontSize(GameObject gameObject, float multipler)
	{
		Text[] componentsInChildren = gameObject.GetComponentsInChildren<Text>();
		Text[] array = componentsInChildren;
		foreach (Text text in array)
		{
			text.fontSize = (int)((float)text.fontSize * multipler);
		}
	}

	public static float GetWidthScale()
	{
		if (widthScale.CompareTo(0f) == 0)
		{
			widthScale = (float)Screen.width / 960f;
		}
		return widthScale;
	}

	public static float GetHeightScale()
	{
		if (heightScale.CompareTo(0f) == 0)
		{
			heightScale = (float)Screen.height / 640f;
		}
		return heightScale;
	}

	public static int GetReferencedScreenHeight()
	{
		return 640;
	}

	public static int GetReferencedScreenWidth()
	{
		return 960;
	}

	public static string FormatTime(int time, ILocalizationService localizationService)
	{
		return FormatTime(Convert.ToDouble(time), localizationService);
	}

	public static string FormatTime(double time, ILocalizationService localizationService)
	{
		int num = (int)time / 86400;
		int num2 = (int)time / 3600 % 24;
		int num3 = (int)time / 60 % 60;
		int num4 = (int)time % 60;
		if (num > 2)
		{
			return string.Format(localizationService.GetString("TimerDaysLeft"), num);
		}
		if (num > 0)
		{
			if (num2 > 0)
			{
				return string.Format("{0:0}{1} {2:0}{3}", num, localizationService.GetString("TimerDaysAbbreviation"), num2, localizationService.GetString("TimerHoursAbbreviation"));
			}
			return string.Format("{0:0}{1}", num, localizationService.GetString("TimerDaysAbbreviation"));
		}
		if (num2 > 0)
		{
			if (num3 > 0)
			{
				return string.Format("{0:0}{1} {2:0}{3}", num2, localizationService.GetString("TimerHoursAbbreviation"), num3, localizationService.GetString("TimerMinutesAbbreviation"));
			}
			return string.Format("{0:0}{1}", num2, localizationService.GetString("TimerHoursAbbreviation"));
		}
		if (num3 > 0)
		{
			if (num4 > 0)
			{
				return string.Format("{0:0}{1} {2:0}{3}", num3, localizationService.GetString("TimerMinutesAbbreviation"), num4, localizationService.GetString("TimerSecondsAbbreviation"));
			}
			return string.Format("{0:0}{1}", num3, localizationService.GetString("TimerMinutesAbbreviation"));
		}
		return string.Format("{0:0}{1}", num4, localizationService.GetString("TimerSecondsAbbreviation"));
	}

	public static Tuple<int, int, int, int> DigitalClockValues(float time)
	{
		int time2 = Mathf.FloorToInt(time);
		return DigitalClockValues(time2);
	}

	public static Tuple<int, int, int, int> DigitalClockValues(int time)
	{
		int num = time / 3600 % 24;
		int num2 = time / 60 % 60;
		int num3 = time % 60;
		Tuple<int, int, int, int> tuple = new Tuple<int, int, int, int>(0, 0, 0, 0);
		if (num > 0)
		{
			tuple.Item1 = num / 10;
			tuple.Item2 = num % 10;
			tuple.Item3 = num2 / 10;
			tuple.Item4 = num2 % 10;
			return tuple;
		}
		tuple.Item1 = num2 / 10;
		tuple.Item2 = num2 % 10;
		tuple.Item3 = num3 / 10;
		tuple.Item4 = num3 % 10;
		return tuple;
	}

	public static string FormatSocialTime(double time)
	{
		int num = (int)time / 3600;
		int num2 = (int)time / 60 % 60;
		int num3 = (int)time % 60;
		return string.Format("{0:0}:{1:0}:{2:0}", num, num2, num3);
	}

	public static string FormatDate(int timestamp, ILocalizationService localizationService)
	{
		return GameConstants.Timers.epochStart.AddSeconds(timestamp).ToString("d", localizationService.CultureInfo);
	}

	public static List<string> BreakDialog(string targetText, int pieceCount, char[] languageDelimiters, IKampaiLogger logger)
	{
		List<int> list = new List<int>();
		List<string> list2 = new List<string>();
		if (string.IsNullOrEmpty(targetText))
		{
			return list2;
		}
		list.Add(0);
		int startIndex = 0;
		for (int i = 1; i < pieceCount; i++)
		{
			int num = targetText.Length / pieceCount * i;
			int item = FindCloestIndexOfWordBreaker(targetText, startIndex, num, languageDelimiters, logger);
			list.Add(item);
			startIndex = num;
		}
		list.Add(targetText.Length);
		for (int j = 0; j < list.Count - 1; j++)
		{
			string item2 = targetText.Substring(list[j] + ((j != 0) ? 1 : 0), list[j + 1] - list[j] + ((j != list.Count - 2) ? 1 : (-1)));
			list2.Add(item2);
		}
		return list2;
	}

	private static int FindCloestIndexOfWordBreaker(string targetText, int startIndex, int endIndex, char[] languageDelimiters, IKampaiLogger logger)
	{
		int num = -1;
		int balanceIndex = -1;
		bool closeTagIsInFront = false;
		List<int> leftBracketList = null;
		int num2 = HTMLTagValidator(targetText, startIndex, endIndex, out balanceIndex, out closeTagIsInFront, out leftBracketList, logger);
		if (num2 == 0)
		{
			if (closeTagIsInFront)
			{
				num = ((balanceIndex != -1) ? balanceIndex : targetText.LastIndexOf('<', endIndex, endIndex - startIndex));
			}
			else
			{
				num = targetText.LastIndexOfAny(languageDelimiters, endIndex, endIndex - startIndex);
				if (balanceIndex != -1 && num < balanceIndex)
				{
					num = balanceIndex;
				}
			}
		}
		else
		{
			if (leftBracketList == null || leftBracketList.Count == 0)
			{
				return endIndex;
			}
			bool flag = false;
			if (num2 > 0)
			{
				for (int num3 = leftBracketList.Count - 1; num3 >= 0; num3--)
				{
					if (!flag && leftBracketList[num3] >= 0)
					{
						flag = true;
					}
					else if (flag && leftBracketList[num3] <= 0)
					{
						num = leftBracketList[num3 + 1] - 1;
						break;
					}
				}
				if (flag && num == -1)
				{
					num = leftBracketList[0] - 1;
				}
			}
			else
			{
				int leftAngleBracketIndex = 0;
				for (int i = 0; i < leftBracketList.Count; i++)
				{
					if (!flag && leftBracketList[i] <= 0)
					{
						flag = true;
					}
					else if (flag && leftBracketList[i] >= 0)
					{
						leftAngleBracketIndex = leftBracketList[i + 1];
						break;
					}
				}
				if (flag && num == -1)
				{
					leftAngleBracketIndex = leftBracketList[leftBracketList.Count - 1];
				}
				num = FindRightAngleBracket(leftAngleBracketIndex, targetText, logger);
			}
		}
		if (num == -1)
		{
			return endIndex;
		}
		return num;
	}

	public static int HTMLTagValidator(string targetString, int startIndex, int endIndex, out int balanceIndex, out bool closeTagIsInFront, out List<int> leftBracketList, IKampaiLogger logger)
	{
		balanceIndex = -1;
		closeTagIsInFront = false;
		leftBracketList = new List<int>();
		if (endIndex <= startIndex)
		{
			return 0;
		}
		int num = 0;
		bool flag = false;
		for (int i = startIndex; i <= endIndex; i++)
		{
			if (targetString[i] != '<')
			{
				continue;
			}
			if (targetString[i + 1] == '/')
			{
				num--;
				leftBracketList.Add(-i);
				if (!flag)
				{
					closeTagIsInFront = true;
				}
			}
			else
			{
				leftBracketList.Add(i);
				num++;
			}
			if (num == 0 || (closeTagIsInFront && flag && num == -1))
			{
				balanceIndex = FindRightAngleBracket(i, targetString, logger);
			}
			flag = true;
		}
		return num;
	}

	private static int FindRightAngleBracket(int leftAngleBracketIndex, string text, IKampaiLogger logger)
	{
		int result = -1;
		try
		{
			for (int i = leftAngleBracketIndex; i < text.Length; i++)
			{
				if (text[i] == '>')
				{
					result = ((i == text.Length - 1) ? i : ((text[i + 1] != '<') ? (i + 1) : i));
					break;
				}
			}
		}
		catch (IndexOutOfRangeException ex)
		{
			logger.Log(KampaiLogLevel.Warning, "UIUtils FindRightAngleBracket failed. LeftAngleBracketIndex: {0}. Text: {1}. With exception:{3}", leftAngleBracketIndex.ToString(), text, ex.StackTrace);
		}
		return result;
	}

	public static void FlashingColor(Text text, int index)
	{
		text.color = GameConstants.UI.UI_BOOSTCOLOR;
		index = 0;
	}

	public static Sprite LoadSpriteFromPath(string path, string defaultImage = "btn_Circle01_mask")
	{
		Sprite sprite = KampaiResources.Load<Sprite>(path);
		if (sprite == null)
		{
			sprite = KampaiResources.Load<Sprite>(defaultImage);
		}
		return sprite;
	}

	public static void SetItemIcon(KampaiImage itemImage, ItemDefinition itemDefinitions)
	{
		if (itemDefinitions != null)
		{
			SetItemIcon(itemImage, (IDisplayableDefinition)itemDefinitions);
		}
	}

	public static void SetItemIcon(KampaiImage itemImage, IDisplayableDefinition itemDefinitions)
	{
		if (!(itemImage == null))
		{
			object displayableDefinition = itemDefinitions;
			if (displayableDefinition == null)
			{
				DisplayableDefinition displayableDefinition2 = new DisplayableDefinition();
				displayableDefinition2.Image = "btn_Main01_fill";
				displayableDefinition2.Mask = "btn_Main01_mask";
				displayableDefinition = displayableDefinition2;
			}
			itemDefinitions = (IDisplayableDefinition)displayableDefinition;
			string text = itemDefinitions.Image;
			if (string.IsNullOrEmpty(text))
			{
				text = "btn_Main01_fill";
			}
			Sprite sprite = LoadSpriteFromPath(text);
			itemImage.sprite = sprite;
			string text2 = itemDefinitions.Mask;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "btn_Main01_mask";
			}
			Sprite maskSprite = LoadSpriteFromPath(text2);
			itemImage.maskSprite = maskSprite;
		}
	}

	public static string FormatLargeNumber(int number)
	{
		if (number == 0)
		{
			return number.ToString();
		}
		NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
		numberFormatInfo.NumberGroupSeparator = " ";
		return number.ToString("#,#", numberFormatInfo);
	}

	public static int GetIntFromFormattedLargeNumber(string stringNumber)
	{
		string s = Regex.Replace(stringNumber, "\\s+", string.Empty);
		int result;
		int.TryParse(s, out result);
		return result;
	}

	public static void SafeDestoryViews<T>(IList<T> views) where T : MonoBehaviour
	{
		if (views == null || views.Count == 0)
		{
			return;
		}
		T val = (T)null;
		for (int i = 0; i < views.Count; i++)
		{
			val = views[i];
			if (!(val == null) && !(val.gameObject == null))
			{
				val.gameObject.SetActive(false);
				UnityEngine.Object.Destroy(val.gameObject);
			}
		}
		views.Clear();
	}

	public static void SafeDestoryViews<T>(T[] views) where T : MonoBehaviour
	{
		if (views == null || views.Length == 0)
		{
			return;
		}
		T val = (T)null;
		for (int i = 0; i < views.Length; i++)
		{
			val = views[i];
			if (!(val == null) && !(val.gameObject == null))
			{
				val.gameObject.SetActive(false);
				UnityEngine.Object.Destroy(val.gameObject);
			}
		}
	}
}

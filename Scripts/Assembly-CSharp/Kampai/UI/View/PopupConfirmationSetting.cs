using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class PopupConfirmationSetting
	{
		public string TitleKey;

		public string DescriptionKey;

		public bool DescriptionAlreadyTranslated;

		public string ImagePath;

		public Signal<bool> ConfirmationCallback;

		public string LeftButtonText;

		public string RightButtonText;

		public PopupConfirmationSetting(string titleKey, string descKey, string imageToDisplay, Signal<bool> callback)
		{
			TitleKey = titleKey;
			DescriptionKey = descKey;
			ImagePath = imageToDisplay;
			ConfirmationCallback = callback;
			LeftButtonText = string.Empty;
			RightButtonText = string.Empty;
		}

		public PopupConfirmationSetting(string titleKey, string descKey, bool descTranslated, string imageToDisplay, Signal<bool> callback, string leftButtonText, string rightButtonText)
		{
			TitleKey = titleKey;
			DescriptionKey = descKey;
			DescriptionAlreadyTranslated = descTranslated;
			ImagePath = imageToDisplay;
			ConfirmationCallback = callback;
			LeftButtonText = leftButtonText;
			RightButtonText = rightButtonText;
		}
	}
}

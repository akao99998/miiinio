using System.Collections;
using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestDialogView : KampaiView
	{
		public Text DialogText;

		public RectTransform DialogBackground;

		public RectTransform QuestTextTransform;

		public ButtonView QuestButton;

		public KampaiImage DialogIcon;

		public RectTransform NextArrow;

		private string dialogText;

		private int currentPageIndex;

		private IList<string> dialogPages;

		private ILocalizationService localizationService;

		private IEnumerator processDialogEnumerator;

		public void Init(ILocalizationService localizationService)
		{
			this.localizationService = localizationService;
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;
			dialogPages = new List<string>();
			base.gameObject.SetActive(false);
		}

		internal void SetDialogIcon(string maskPath)
		{
			DialogIcon.maskSprite = UIUtils.LoadSpriteFromPath(maskPath);
		}

		public void ShowPreviousDialog()
		{
			if (dialogPages.Count != 0)
			{
				currentPageIndex = 0;
				UpdateDialog();
			}
			base.gameObject.SetActive(true);
		}

		public void ShowDialog(string dialog, IKampaiLogger logger)
		{
			currentPageIndex = 0;
			dialogPages.Clear();
			dialogText = dialog;
			DialogText.text = dialog;
			MoveOffDialog();
			base.gameObject.SetActive(true);
			processDialogEnumerator = ProcessDialog(logger);
			StartCoroutine(processDialogEnumerator);
		}

		protected override void OnDestroy()
		{
			if (processDialogEnumerator != null)
			{
				StopCoroutine(processDialogEnumerator);
				processDialogEnumerator = null;
			}
			base.OnDestroy();
		}

		public bool IsPageOver()
		{
			if (dialogPages.Count == 0 || currentPageIndex == dialogPages.Count)
			{
				return true;
			}
			return false;
		}

		public void UpdateDialog()
		{
			DialogText.text = dialogPages[currentPageIndex++];
			StartCoroutine(UpdateDisplay());
		}

		private void ProcessText(IKampaiLogger logger)
		{
			if (QuestTextTransform != null)
			{
				float height = QuestTextTransform.rect.height;
				if (DialogSizeCheck(height))
				{
					ProcessDialog_Normal();
				}
				else
				{
					BreakDialogByTextBoxSize(height, logger);
				}
			}
		}

		private void ProcessDialog_Normal()
		{
			dialogText = string.Empty;
			MoveBackDialog();
		}

		private void BreakDialogByTextBoxSize(float height, IKampaiLogger logger)
		{
			int pieceCount = Mathf.CeilToInt(height / DialogBackground.rect.height);
			string text = localizationService.GetString("PunctuationDelimiters");
			if (string.IsNullOrEmpty(text))
			{
				text = ",.?!";
			}
			text += " \n\r\t>";
			char[] languageDelimiters = text.ToCharArray();
			dialogPages = UIUtils.BreakDialog(dialogText, pieceCount, languageDelimiters, logger);
			UpdateDialog();
		}

		private bool DialogSizeCheck(float height)
		{
			if (string.IsNullOrEmpty(dialogText))
			{
				return true;
			}
			if (DialogBackground.rect.height < height)
			{
				return false;
			}
			return true;
		}

		private IEnumerator ProcessDialog(IKampaiLogger logger)
		{
			yield return null;
			yield return null;
			ProcessText(logger);
			processDialogEnumerator = null;
		}

		private IEnumerator UpdateDisplay()
		{
			yield return null;
			if (currentPageIndex == 1)
			{
				MoveBackDialog();
			}
		}

		private void MoveOffDialog()
		{
			base.transform.localPosition = new Vector3(0f, 2 * Screen.height, 0f);
		}

		private void MoveBackDialog()
		{
			base.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
	}
}

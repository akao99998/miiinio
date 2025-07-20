using System.Collections;
using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class CreditsPanelMediator : UIStackMediator<CreditsPanelView>
	{
		private List<GameObject> textObjects = new List<GameObject>();

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public ILocalizationService locService { get; set; }

		[Inject]
		public IClientVersion clientVersion { get; set; }

		public override void OnRegister()
		{
			base.view.closeButton.ClickedSignal.AddListener(CloseButton);
			base.OnRegister();
		}

		public override void OnRemove()
		{
			base.view.closeButton.ClickedSignal.RemoveListener(CloseButton);
			base.OnRemove();
		}

		private void CloseButton()
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			Close();
		}

		protected override void Close()
		{
			base.gameObject.SetActive(false);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (base.view != null)
			{
				Start();
			}
		}

		private void Start()
		{
			base.view.scrollRect.verticalNormalizedPosition = 1f;
			for (int i = 0; i < 31; i++)
			{
				string key = string.Format("{0}{1:00}", "CreditContents", i);
				if (locService.Contains(key))
				{
					string @string = locService.GetString(key);
					if (!string.IsNullOrEmpty(@string))
					{
						PopulateCredits(@string);
					}
				}
			}
			base.view.creditText.text = locService.GetString("CreditContents", "©", clientVersion.GetClientVersion());
			base.view.overlayImage.gameObject.SetActive(true);
			StartCoroutine(SetupDivisionsCoroutine());
			StartCoroutine(HideOverlayCoroutine());
		}

		private IEnumerator SetupDivisionsCoroutine()
		{
			yield return new WaitForEndOfFrame();
			base.view.SetupDivisions(base.view.creditText.rectTransform.rect.height);
		}

		private IEnumerator HideOverlayCoroutine()
		{
			yield return null;
			base.view.overlayImage.gameObject.SetActive(false);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			base.view.Cleanup();
			foreach (GameObject textObject in textObjects)
			{
				Object.Destroy(textObject);
			}
		}

		private void PopulateCredits(string content)
		{
			GameObject gameObject = Object.Instantiate(base.view.creditText.gameObject);
			gameObject.transform.SetParent(base.view.scrollRect.content.transform, false);
			textObjects.Add(gameObject);
			Text component = gameObject.GetComponent<Text>();
			component.text = content;
			base.view.textList.Add(component);
		}
	}
}

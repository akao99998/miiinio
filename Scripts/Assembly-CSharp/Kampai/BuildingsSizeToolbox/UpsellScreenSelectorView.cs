using System.Collections;
using Kampai.UI.View.UpSell;
using Kampai.Util;
using UnityEngine;

namespace Kampai.BuildingsSizeToolbox
{
	public class UpsellScreenSelectorView : KampaiView
	{
		private static readonly string[] KnownScreens = new string[4] { "screen_Upsell1Pack", "screen_Upsell2Pack", "screen_Upsell3Pack", "screen_Upsell4Pack" };

		public GameObject ScreenParent;

		public GameObject ScrollContent;

		public UpsellScreenSelectorListItemView ListItemViewBase;

		private GameObject currentScreen;

		[Inject]
		public NewUpsellScreenSelectedSignal newUpsellScreenSelected { get; set; }

		protected override void Start()
		{
			base.Start();
			string[] knownScreens = KnownScreens;
			foreach (string text in knownScreens)
			{
				UpsellScreenSelectorListItemView upsellScreenSelectorListItemView = Object.Instantiate(ListItemViewBase);
				upsellScreenSelectorListItemView.ScreenName.text = text;
				upsellScreenSelectorListItemView.gameObject.SetActive(true);
				upsellScreenSelectorListItemView.transform.SetParent(ScrollContent.transform, false);
				upsellScreenSelectorListItemView.ClickedSignal.AddListener(LoadScreen);
			}
			StartCoroutine(LoadFirstScreen());
		}

		private IEnumerator LoadFirstScreen()
		{
			yield return null;
			LoadScreen(KnownScreens[0]);
		}

		private void LoadScreen(string name)
		{
			if (currentScreen != null)
			{
				Object.Destroy(currentScreen);
			}
			Object original = Resources.Load("UI/UI_Prefabs/UI_Common/Features/" + name);
			GameObject gameObject = Object.Instantiate(original) as GameObject;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.SetParent(ScreenParent.transform, false);
			rectTransform.localPosition = Vector2.zero;
			rectTransform.sizeDelta = Vector2.zero;
			UpSellModalView component = gameObject.GetComponent<UpSellModalView>();
			component.Init();
			component.Open();
			currentScreen = gameObject;
			newUpsellScreenSelected.Dispatch(component);
		}
	}
}

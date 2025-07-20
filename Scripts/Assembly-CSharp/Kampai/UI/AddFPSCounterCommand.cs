using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.command.impl;

namespace Kampai.UI
{
	internal sealed class AddFPSCounterCommand : Command
	{
		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject GlassCanvas { get; set; }

		[Inject]
		public bool Show { get; set; }

		[Inject]
		public int SampleSize { get; set; }

		public override void Execute()
		{
			if (Show)
			{
				ShowCounter();
			}
			else
			{
				HideCounter();
			}
		}

		private void HideCounter()
		{
			KampaiFPSCounter componentInChildren = GlassCanvas.gameObject.GetComponentInChildren<KampaiFPSCounter>();
			if (!(componentInChildren == null))
			{
				Object.Destroy(componentInChildren.gameObject);
			}
		}

		private void ShowCounter()
		{
			KampaiFPSCounter componentInChildren = GlassCanvas.gameObject.GetComponentInChildren<KampaiFPSCounter>();
			if (componentInChildren != null)
			{
				if (componentInChildren.SampleInterval == SampleSize)
				{
					return;
				}
				Object.DestroyImmediate(componentInChildren.gameObject);
			}
			GameObject gameObject = new GameObject("FPSCounter", typeof(RectTransform));
			gameObject.transform.SetParent(GlassCanvas.transform, false);
			Text text = gameObject.AddComponent<Text>();
			text.rectTransform.sizeDelta = Vector2.zero;
			text.rectTransform.anchorMin = new Vector2(0.4f, 0.002f);
			text.rectTransform.anchorMax = new Vector2(0.8f, 0.1f);
			text.rectTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
			text.text = "30";
			text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
			text.fontSize = 40;
			text.color = Color.black;
			text.alignment = TextAnchor.LowerCenter;
			componentInChildren = gameObject.AddComponent<KampaiFPSCounter>();
			componentInChildren.TextComponent = text;
			if (SampleSize > 0)
			{
				componentInChildren.SampleInterval = SampleSize;
			}
		}
	}
}

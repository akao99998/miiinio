using System;
using System.IO;
using Kampai.Common;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class HindsightContentView : strange.extensions.mediation.impl.View
	{
		private string campaignUri;

		public HindsightCampaignDefinition definition { get; set; }

		public HideSkrimSignal hideSkrimSignal { get; set; }

		public HindsightContentDismissSignal dismissSignal { get; set; }

		public IGUIService guiService { get; set; }

		public ITelemetryService telemetryService { get; set; }

		public void Open(GameObject glassCanvas, HindsightCampaign campaign, string languageKey)
		{
			campaignUri = HindsightUtil.GetUri(campaign.Definition, languageKey);
			string contentCachePath = HindsightUtil.GetContentCachePath(campaign.Definition, languageKey);
			Texture2D texture2D = LoadImage(contentCachePath);
			if (texture2D == null)
			{
				CloseContent();
				return;
			}
			KampaiClickableImage component = GetComponent<KampaiClickableImage>();
			component.material.SetTexture("_MainTex", texture2D);
			component.ClickedSignal.AddListener(OnClick);
			component.EnableClick(true);
			float referenceScale = 1f;
			CanvasScaler component2 = glassCanvas.GetComponent<CanvasScaler>();
			if (component2 != null)
			{
				float num = Mathf.Lerp(component2.referenceResolution.x, component2.referenceResolution.y, component2.matchWidthOrHeight);
				float num2 = Mathf.Lerp(Screen.width, Screen.height, component2.matchWidthOrHeight);
				referenceScale = num / num2;
			}
			ScaleImage(texture2D, component, referenceScale);
			component.SetMaterialDirty();
		}

		public void Close(HindsightCampaign.DismissType dismissType)
		{
			HindsightCampaign.Scope type = (HindsightCampaign.Scope)(int)Enum.Parse(typeof(HindsightCampaign.Scope), definition.Scope);
			dismissSignal.Dispatch(type, dismissType);
			telemetryService.Send_Telemetry_EVT_IN_APP_MESSAGE_DISPLAYED(definition.Scope, dismissType);
			CloseContent();
			KampaiClickableImage component = GetComponent<KampaiClickableImage>();
			UnityEngine.Object.Destroy(component.mainTexture);
			component.ClickedSignal.RemoveListener(OnClick);
		}

		private Texture2D LoadImage(string filePath)
		{
			if (File.Exists(filePath))
			{
				Texture2D texture2D = new Texture2D(1, 1);
				byte[] data = File.ReadAllBytes(filePath);
				if (texture2D.LoadImage(data))
				{
					return texture2D;
				}
			}
			return null;
		}

		private void ScaleImage(Texture2D texture, Image image, float referenceScale)
		{
			int width = texture.width;
			int height = texture.height;
			int num = Screen.width - 100;
			int num2 = Screen.height - 100;
			if (num - width >= 0 && num2 - height >= 0)
			{
				image.rectTransform.sizeDelta = new Vector2((float)width * referenceScale, (float)height * referenceScale);
				return;
			}
			float num3 = (float)num / (float)width;
			float num4 = (float)num2 / (float)height;
			float num5 = ((!(num3 <= num4)) ? num4 : num3);
			image.rectTransform.sizeDelta = new Vector2((float)width * referenceScale * num5, (float)height * referenceScale * num5);
		}

		private void OnClick()
		{
			Close(HindsightCampaign.DismissType.ACCEPTED);
			Application.OpenURL(campaignUri);
		}

		private void CloseContent()
		{
			hideSkrimSignal.Dispatch("HindsightContentSkrim");
			IGUICommand command = guiService.BuildCommand(GUIOperation.Unload, "HindsightContentView");
			guiService.Execute(command);
		}
	}
}

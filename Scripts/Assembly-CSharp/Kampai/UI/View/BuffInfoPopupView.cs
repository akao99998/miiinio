using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class BuffInfoPopupView : PopupMenuView, IGenericPopupView
	{
		public KampaiImage BuffIcon;

		public Text BuffAmount;

		public Text BuffDescription;

		public Text CharacterName;

		public GameObject pointer;

		public MinionSlotModal MinionSlot;

		public float Duration = 3f;

		private ILocalizationService localService;

		private float xBuffer = 0.02f;

		public void Init(ILocalizationService localizationService)
		{
			base.Init();
			localService = localizationService;
		}

		public void Display(Vector3 itemCenter)
		{
			base.gameObject.transform.position = itemCenter;
			base.Open();
		}

		public void SetOffset(float yInput, GameObject glassCanvas, Vector3 itemCenter)
		{
			Vector3 zero = Vector3.zero;
			base.transform.localPosition = zero;
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchorMin = itemCenter;
			rectTransform.anchorMax = itemCenter;
			RectTransform rectTransform2 = glassCanvas.transform as RectTransform;
			float x = rectTransform2.sizeDelta.x;
			float y = rectTransform2.sizeDelta.y;
			float num = rectTransform.sizeDelta.x / x / 2f;
			float num2 = rectTransform.sizeDelta.y / y / 2f;
			RectTransform rectTransform3 = pointer.transform as RectTransform;
			float num3 = rectTransform3.sizeDelta.y / y / 2f;
			float num4 = 0f;
			float num5 = rectTransform.anchorMin.x - num;
			float num6 = rectTransform.anchorMax.x + num;
			if (num5 < 0f)
			{
				num4 = 0f - num5 + xBuffer;
			}
			else if (num6 > 1f)
			{
				num4 = 1f - num6 - xBuffer;
			}
			float num7 = 0f - num2 - num3 * 2f + yInput / y;
			float num8 = 90f;
			float num9 = rectTransform3.anchorMin.y;
			if (yInput > 0f)
			{
				num7 = 0f - num7;
				num8 = 0f - num8;
				num9 = 0f - (num9 - 1f);
			}
			rectTransform.anchorMin += new Vector2(num4, num7);
			rectTransform.anchorMax += new Vector2(num4, num7);
			pointer.transform.eulerAngles = new Vector3(0f, 0f, num8);
			float num10 = num4 * x / rectTransform.sizeDelta.x;
			rectTransform3.anchorMin = new Vector2(rectTransform3.anchorMin.x - num10, num9);
			rectTransform3.anchorMax = new Vector2(rectTransform3.anchorMax.x - num10, num9);
		}

		internal void SetBuff(BuffDefinition def, float currentMultiplier)
		{
			BuffIcon.maskSprite = UIUtils.LoadSpriteFromPath(def.buffSimpleMask);
			BuffDescription.text = localService.GetString(def.buffDetailLocalizedKey);
			BuffAmount.text = localService.GetString("partyBuffMultiplier", currentMultiplier);
		}

		internal void SetGuestName(string localizationKey)
		{
			CharacterName.text = localService.GetString(localizationKey);
		}
	}
}

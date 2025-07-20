using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class DebrisModalItemView : KampaiView, IDragHandler, IEventSystemHandler
	{
		public GameObject BackingOn;

		public GameObject BackingOff;

		public Text QuantityText;

		public RectTransform DragContainer;

		public KampaiImage Image;

		public GameObject GlowBacking;

		public KampaiImage DragPromptItem;

		private Vector2 initialPosition;

		private Camera uiCamera;

		internal void Init(Camera uiCamera)
		{
			this.uiCamera = uiCamera;
		}

		internal void Init(string image, string mask, int amountAvailble, int amountRequired)
		{
			Image.sprite = UIUtils.LoadSpriteFromPath(image);
			Image.maskSprite = UIUtils.LoadSpriteFromPath(mask);
			DragPromptItem.sprite = Image.sprite;
			DragPromptItem.maskSprite = Image.maskSprite;
			UpdateQuantity(amountAvailble, amountRequired);
			initialPosition = DragContainer.anchoredPosition;
			Highlight(false);
		}

		internal void UpdateQuantity(int quantity, int quantityRequired)
		{
			QuantityText.text = string.Format("{0}/{1}", quantity, quantityRequired);
			bool flag = quantity >= quantityRequired;
			BackingOn.SetActive(flag);
			BackingOff.SetActive(!flag);
			DragPromptItem.gameObject.SetActive(flag);
		}

		public void OnDrag(PointerEventData eventData)
		{
			DragContainer.position = uiCamera.ScreenToWorldPoint(eventData.position);
			DragContainer.localPosition = VectorUtils.ZeroZ(DragContainer.localPosition);
			DragContainer.localPosition = new Vector2(DragContainer.localPosition.x, DragContainer.localPosition.y);
		}

		public void ResetPosition(bool animate)
		{
			if (!animate)
			{
				DragContainer.anchoredPosition = initialPosition;
				return;
			}
			Go.killAllTweensWithTarget(base.transform);
			Go.to(DragContainer, 0.25f, new GoTweenConfig().setEaseType(GoEaseType.Linear).vector2Prop("anchoredPosition", initialPosition).onComplete(delegate(AbstractGoTween thisTween)
			{
				thisTween.destroy();
			}));
		}

		public void Highlight(bool enable)
		{
			GlowBacking.SetActive(enable);
		}
	}
}

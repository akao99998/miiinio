using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.api;

namespace Kampai.UI.View
{
	public class StorageBuildingModalView : PopupMenuView
	{
		private const int SELL_PANEL_OPEN_COLUMN_NUM = 3;

		private const float REARRANGE_ITEM_TIME = 0.3f;

		private const float INSTANCE_REARRANGE_ITEM_TIME = 0.0001f;

		private const float SELL_PANEL_OPEN_ANCHOR = 0.52f;

		public ButtonView UpgradeButtonView;

		public ButtonView SellButtonView;

		public ButtonView BuyButtonView;

		public ButtonView ScrollListButtonView;

		public RectTransform ItemsPanel;

		public KampaiImage SellGrayImage;

		public KampaiImage BuyGrayImage;

		public KampaiImage BuyBackgroundImage;

		public KampaiImage SellBackgroundImage;

		public KampaiLabel InfoLabel;

		public Text Capacity;

		public KampaiScrollView scrollView;

		public RectTransform FillImage;

		public Animator backgroundAnim;

		public GameObject CapacityPanel;

		private int currentItemCount;

		private int currentCapacity;

		internal SellPanelView SellPanel { get; set; }

		internal BuyMarketplacePanelView BuyPanel { get; set; }

		public override void Init()
		{
			base.Init();
			Open();
		}

		internal void RearrangeItemView(bool isMoveInstance = false)
		{
			int num = 0;
			int num2 = Mathf.FloorToInt(scrollView.ColumnNumber);
			float num3 = 1f;
			if (SellPanel != null && SellPanel.isOpen)
			{
				num2 = 3;
				num3 = 0.52f;
			}
			int num4 = 0;
			int num5 = 0;
			foreach (MonoBehaviour itemView in scrollView.ItemViewList)
			{
				StorageBuildingItemView storageBuildingItemView = itemView as StorageBuildingItemView;
				RectTransform rectTransform = itemView.transform as RectTransform;
				if (!(storageBuildingItemView == null) && !(rectTransform == null))
				{
					num4 = num / num2;
					num5 = num % num2;
					float num6 = rectTransform.anchorMax.x - rectTransform.anchorMin.x;
					Vector2 newOffsetMin = new Vector2(0f, (float)(-num4 - 1) * scrollView.ItemSize);
					Vector2 newOffsetMax = new Vector2(0f, (float)(-num4) * scrollView.ItemSize);
					Vector2 newAnchorMin = new Vector2(num3 / (float)num2 * (float)num5, 1f);
					storageBuildingItemView.MoveToAnchorOffset(newAnchorMax: new Vector2(newAnchorMin.x + num6, 1f), moveTime: (!isMoveInstance) ? 0.3f : 0.0001f, newOffsetMin: newOffsetMin, newOffsetMax: newOffsetMax, newAnchorMin: newAnchorMin);
					num++;
				}
			}
			scrollView.SetupScrollView(num2, KampaiScrollView.MoveDirection.Start);
		}

		internal void EnableMarketplace(bool isEnabled)
		{
			BuyGrayImage.gameObject.SetActive(!isEnabled);
			SellGrayImage.gameObject.SetActive(!isEnabled);
			BuyBackgroundImage.gameObject.SetActive(isEnabled);
			SellBackgroundImage.gameObject.SetActive(isEnabled);
		}

		internal void LoadSellMarketplacePanel()
		{
			SellPanel = AddMarketplacePanel<SellPanelView>("cmp_marketPlaceSellPanel");
		}

		internal void LoadBuyMarketplacePanel()
		{
			BuyPanel = AddMarketplacePanel<BuyMarketplacePanelView>("cmp_marketPlaceBuyPanel");
		}

		private T AddMarketplacePanel<T>(string prefabName) where T : MonoBehaviour, IView
		{
			GameObject original = KampaiResources.Load(prefabName) as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			if (gameObject == null)
			{
				return (T)null;
			}
			RectTransform rectTransform = gameObject.transform as RectTransform;
			if (rectTransform == null)
			{
				return gameObject.GetComponent<T>();
			}
			rectTransform.SetParent(ItemsPanel, false);
			return gameObject.GetComponent<T>();
		}

		internal void DisableExpandButton()
		{
			UpgradeButtonView.gameObject.SetActive(false);
		}

		internal void UpdateProgressBar()
		{
			float progressBar = (float)currentItemCount / (float)currentCapacity;
			SetProgressBar(progressBar);
			if (backgroundAnim != null)
			{
				if (currentCapacity - currentItemCount == 0)
				{
					backgroundAnim.Play("Full");
				}
				else if (currentCapacity - currentItemCount < 10)
				{
					backgroundAnim.Play("AlmostFull");
				}
				else
				{
					backgroundAnim.Play("Init");
				}
			}
		}

		internal void SetCap(int cap)
		{
			currentCapacity = cap;
			UpdateProgressBar();
		}

		internal void SetCurrentItemCount(int itemCount)
		{
			currentItemCount = itemCount;
			UpdateProgressBar();
		}

		internal void UpdateStorageStatus(bool isStorageFull)
		{
			Capacity.text = string.Format("{0}/{1}", currentItemCount, currentCapacity);
			HighlightExpand(isStorageFull);
		}

		internal void HighlightExpand(bool isHighlighted)
		{
			HighlightButton(isHighlighted, UpgradeButtonView);
		}

		internal void HighlightSellButton(bool isHighlighted)
		{
			HighlightButton(isHighlighted, SellButtonView);
		}

		internal void HighlightBuyButton(bool isHighlighted)
		{
			HighlightButton(isHighlighted, BuyButtonView);
		}

		internal void HighlightButton(bool isHighlighted, ButtonView highlightedButton)
		{
			Animator[] componentsInChildren = highlightedButton.GetComponentsInChildren<Animator>();
			if (isHighlighted)
			{
				Animator[] array = componentsInChildren;
				foreach (Animator animator in array)
				{
					animator.enabled = false;
				}
				Vector3 originalScale = Vector3.one;
				TweenUtil.Throb(highlightedButton.transform, 1.2f, 0.5f, out originalScale);
			}
			else
			{
				Go.killAllTweensWithTarget(highlightedButton.transform);
				highlightedButton.transform.localScale = Vector3.one;
				Animator[] array2 = componentsInChildren;
				foreach (Animator animator2 in array2)
				{
					animator2.enabled = true;
				}
			}
		}

		internal void SetProgressBar(float ratio)
		{
			ratio = Mathf.Clamp(ratio, 0f, 1f);
			FillImage.anchorMax = new Vector2(ratio, FillImage.anchorMax.y);
			RectTransform rectTransform = FillImage.transform as RectTransform;
			if (rectTransform == null)
			{
				return;
			}
			RectTransform rectTransform2 = CapacityPanel.transform as RectTransform;
			if (!(rectTransform2 == null))
			{
				if (currentItemCount == 0)
				{
					Vector2 anchorMin = (rectTransform2.anchorMax = new Vector2(1f / (float)currentCapacity, rectTransform2.anchorMin.y));
					rectTransform2.anchorMin = anchorMin;
				}
				else
				{
					Vector2 anchorMin2 = (rectTransform2.anchorMax = new Vector2(ratio, rectTransform2.anchorMin.y));
					rectTransform2.anchorMin = anchorMin2;
				}
			}
		}
	}
}

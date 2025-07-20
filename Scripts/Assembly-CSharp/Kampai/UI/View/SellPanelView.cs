using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SellPanelView : PopupMenuView
	{
		public ButtonView ArrowButtonView;

		public KampaiScrollView ScrollView;

		public CreateNewSellView CreateSaleView;

		internal bool isOpen;

		internal Signal OnOpenPanelSignal = new Signal();

		internal bool isCreateNewSalePanelOpened;

		protected override void Awake()
		{
			KampaiView.BubbleToContextOnAwake(this, ref currentContext, true);
		}

		internal void FadeAnimation(bool fade)
		{
			isCreateNewSalePanelOpened = fade;
			if (base.animator != null)
			{
				base.animator.Play((!fade) ? "CloseSidePanel" : "OpenSidePanel");
			}
		}

		internal void SetOpen(bool show, bool isInstant = false)
		{
			if (show)
			{
				if (isInstant)
				{
					float lastFrame = 1f;
					int defaultLayer = -1;
					OpenInstantly(defaultLayer, lastFrame);
				}
				else
				{
					Open();
				}
				OnOpenPanelSignal.Dispatch();
			}
			else
			{
				Close();
			}
			isOpen = show;
		}

		public void FadeOutItems()
		{
			FadeItems(false);
		}

		public void FadeInItems()
		{
			FadeItems(true);
		}

		private void FadeItems(bool fadeIn)
		{
			foreach (MonoBehaviour itemView in ScrollView.ItemViewList)
			{
				StorageBuildingSaleSlotView storageBuildingSaleSlotView = itemView as StorageBuildingSaleSlotView;
				if (!(storageBuildingSaleSlotView == null))
				{
					if (fadeIn)
					{
						storageBuildingSaleSlotView.FadeIn();
					}
					else
					{
						storageBuildingSaleSlotView.FadeOut();
					}
				}
			}
		}

		internal bool HasSlot(MarketplaceSaleSlot slot)
		{
			foreach (MonoBehaviour item in ScrollView)
			{
				StorageBuildingSaleSlotView storageBuildingSaleSlotView = item as StorageBuildingSaleSlotView;
				if (storageBuildingSaleSlotView == null || slot.ID != storageBuildingSaleSlotView.slotId)
				{
					continue;
				}
				return true;
			}
			return false;
		}
	}
}

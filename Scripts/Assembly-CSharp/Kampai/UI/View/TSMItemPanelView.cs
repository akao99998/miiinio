using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class TSMItemPanelView : KampaiView
	{
		public UpsellButtonView buttonView;

		public GameObject itemPanel;

		[Header("Prefabs")]
		public GameObject ItemPrefab;

		[SerializeField]
		private float m_layoutSpacing = 25f;

		private readonly IList<TSMQuantityItemView> m_views = new List<TSMQuantityItemView>();

		private IKampaiLogger m_logger;

		private TriggerRewardDefinition m_reward;

		private ILocalizationService m_localizationService;

		private ICurrencyService m_currencyService;

		private IDefinitionService m_definitionService;

		private IUpsellService m_upsellService;

		private IPlayerService m_playerService;

		private Action<TriggerRewardDefinition> m_onPurchaseCallback;

		public void Init(TriggerRewardDefinition reward, Transform parent, ICurrencyService currencyService, ICrossContextCapable gameContext, Action<TriggerRewardDefinition> onPurchaseCallback)
		{
			m_reward = reward;
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			m_logger = LogManager.GetClassLogger("TSMItemPanelView") as IKampaiLogger;
			m_currencyService = currencyService;
			m_definitionService = injectionBinder.GetInstance<IDefinitionService>();
			m_playerService = injectionBinder.GetInstance<IPlayerService>();
			m_localizationService = injectionBinder.GetInstance<ILocalizationService>();
			m_onPurchaseCallback = onPurchaseCallback;
			m_upsellService = injectionBinder.GetInstance<IUpsellService>();
			base.gameObject.transform.SetParent(parent, false);
			SetUpView();
			buttonView.ClickedSignal.AddListener(OnButtonClickCallback);
		}

		private void SetUpView()
		{
			SetUpCollectButton();
			SetItemPanelLayout();
			SetUpItemLayouts(m_reward.layoutElements);
		}

		public void Disable()
		{
			for (int i = 0; i < m_views.Count; i++)
			{
				if (!(m_views[i] == null))
				{
					m_views[i].Disable();
				}
			}
			DisableButton();
		}

		private void OnButtonClickCallback()
		{
			if (buttonView.isDoubleConfirmed())
			{
				Disable();
				m_onPurchaseCallback(m_reward);
			}
		}

		public void TranactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				Disable();
				if (m_onPurchaseCallback != null)
				{
					m_onPurchaseCallback(m_reward);
				}
			}
		}

		public void SetUpCollectButton()
		{
			buttonView.Init(m_upsellService, m_playerService, m_currencyService, m_definitionService, m_localizationService);
			PremiumCurrencyItemDefinition definition = null;
			string sKU = string.Empty;
			if (m_definitionService.TryGet<PremiumCurrencyItemDefinition>(m_reward.SKUId, out definition))
			{
				sKU = definition.SKU;
			}
			buttonView.SetupButton(m_reward.transaction, sKU, 0, m_reward.IsFree, m_reward.buttonText);
		}

		internal void DisableButton()
		{
			buttonView.Disable();
		}

		protected override void OnDestroy()
		{
			Release();
			base.OnDestroy();
		}

		public void Release()
		{
			for (int i = 0; i < m_views.Count; i++)
			{
				ReleaseItem(m_views[i]);
			}
			m_views.Clear();
		}

		private static void ReleaseItem(MonoBehaviour behaviour)
		{
			if (!(behaviour == null) && !(behaviour.gameObject == null))
			{
				UnityEngine.Object.Destroy(behaviour.gameObject);
			}
		}

		public void SetUpItemLayouts(IList<TriggerRewardLayout> layouts)
		{
			if (layouts == null || layouts.Count == 0)
			{
				Transform parent = SetUpLayout(itemPanel, new TriggerRewardLayout(), m_layoutSpacing);
				for (int i = 0; i < m_reward.transaction.GetOutputCount(); i++)
				{
					SetUpItemView(m_reward.transaction.GetOutputItem(i).ID, parent);
				}
				return;
			}
			for (int j = 0; j < layouts.Count; j++)
			{
				TriggerRewardLayout triggerRewardLayout = layouts[j];
				IList<int> itemIds = triggerRewardLayout.itemIds;
				Transform parent2 = SetUpLayout(itemPanel, triggerRewardLayout, m_layoutSpacing);
				for (int k = 0; k < itemIds.Count; k++)
				{
					SetUpItemView(itemIds[k], parent2);
				}
			}
		}

		private void SetItemPanelLayout()
		{
			HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = null;
			switch (m_reward.rewardLayout)
			{
			case TriggerRewardLayout.Layout.None:
			case TriggerRewardLayout.Layout.Horizontal:
				horizontalOrVerticalLayoutGroup = itemPanel.AddComponent<HorizontalLayoutGroup>();
				break;
			case TriggerRewardLayout.Layout.Vertical:
				horizontalOrVerticalLayoutGroup = itemPanel.AddComponent<VerticalLayoutGroup>();
				break;
			}
			if (!(horizontalOrVerticalLayoutGroup == null))
			{
				horizontalOrVerticalLayoutGroup.childForceExpandHeight = false;
				horizontalOrVerticalLayoutGroup.childForceExpandWidth = false;
			}
		}

		public void SetUpItemView(int itemId, Transform parent)
		{
			Definition definition;
			bool flag = m_definitionService.TryGet<Definition>(itemId, out definition);
			if (flag && (!flag || (itemId != 2 && !(definition is UnlockDefinition))))
			{
				TSMQuantityItemView tSMQuantityItemView = SetItemView(ItemPrefab, parent, itemId);
				if (tSMQuantityItemView == null)
				{
					m_logger.Error("Item view is null for item id {0}", itemId);
				}
				else
				{
					m_views.Add(tSMQuantityItemView);
				}
			}
		}

		private TSMQuantityItemView SetItemView(GameObject prefab, Transform parent, int itemId)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			if (gameObject == null)
			{
				m_logger.Error("Couldn't create game object for {0} prefab", prefab);
				return null;
			}
			SetUpItemTransform(gameObject.transform, parent);
			TSMQuantityItemView component = gameObject.GetComponent<TSMQuantityItemView>();
			if (component == null)
			{
				m_logger.Error("Couldn't get TSMQuantityItemView from prefab {0}", prefab);
				UnityEngine.Object.Destroy(gameObject);
				return null;
			}
			QuantityItem outputItemId = m_reward.transaction.GetOutputItemId(itemId);
			if (outputItemId == null)
			{
				m_logger.Error("Couldn't find item id {0} in transaction {1}", itemId, m_reward.transaction);
				UnityEngine.Object.Destroy(gameObject);
				return null;
			}
			return component.Init(m_definitionService.Get<DisplayableDefinition>(outputItemId.ID), outputItemId.Quantity, m_logger, m_localizationService);
		}

		private static void SetUpItemTransform(Transform transform, Transform parent)
		{
			if (!(transform == null))
			{
				transform.SetParent(parent, false);
				RectTransform rectTransform = transform as RectTransform;
				if (!(rectTransform == null))
				{
					rectTransform.anchorMin = Vector2.zero;
					rectTransform.anchorMax = Vector2.one;
					rectTransform.sizeDelta = Vector2.zero;
					rectTransform.localScale = Vector3.one;
					rectTransform.localPosition = Vector3.zero;
				}
			}
		}

		private static Transform SetUpLayout(GameObject itemPanel, TriggerRewardLayout layout, float layoutSpacing)
		{
			Transform transform = null;
			HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = null;
			switch (layout.layout)
			{
			case TriggerRewardLayout.Layout.None:
				return itemPanel.transform;
			case TriggerRewardLayout.Layout.Horizontal:
			{
				GameObject gameObject2 = new GameObject("Horizontal Layout Group");
				horizontalOrVerticalLayoutGroup = gameObject2.AddComponent<HorizontalLayoutGroup>();
				transform = gameObject2.transform;
				break;
			}
			case TriggerRewardLayout.Layout.Vertical:
			{
				GameObject gameObject = new GameObject("Vertical Layout Group");
				horizontalOrVerticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
				transform = gameObject.transform;
				break;
			}
			}
			if (transform == null)
			{
				return null;
			}
			transform.SetParent(itemPanel.transform, false);
			transform.SetSiblingIndex(layout.index);
			if (horizontalOrVerticalLayoutGroup == null)
			{
				return transform;
			}
			horizontalOrVerticalLayoutGroup.childForceExpandHeight = false;
			horizontalOrVerticalLayoutGroup.childForceExpandWidth = false;
			horizontalOrVerticalLayoutGroup.spacing = layoutSpacing;
			return transform;
		}
	}
}

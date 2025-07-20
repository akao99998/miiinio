using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class OrderBoardTicketDetailMediator : Mediator
	{
		private const float waitInBetween = 0.05f;

		public IKampaiLogger logger = LogManager.GetClassLogger("OrderBoardTicketDetailMediator") as IKampaiLogger;

		private int currentPrestigePoints;

		private int neededPrestigePoints;

		private int currentPrestigeLevel;

		private int updateTimes;

		private IEnumerator fillBarRoutine;

		private bool completeFinished;

		private bool closingModal;

		private Action fillOrderCallBack;

		private IEnumerator PointerDownWait;

		private bool showGoto;

		private TransactionInstance ti;

		[Inject]
		public OrderBoardTicketDetailView view { get; set; }

		[Inject]
		public OrderBoardTicketClickedSignal ticketClickedSignal { get; set; }

		[Inject]
		public OrderBoardTicketDeletedSignal ticketDeletedSignal { get; set; }

		[Inject]
		public OrderBoardPrestigeSlotFullSignal slotFullSignal { get; set; }

		[Inject]
		public OrderBoardStartFillingPrestigeBarSignal startFillingPrestigeBarSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetFTUETextSignal setFTUETextSignal { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public DisplayItemPopupSignal displayItemPopupSignal { get; set; }

		[Inject]
		public IFancyUIService fancyUIService { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveAudioListener { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSoundFXSignal { get; set; }

		[Inject]
		public GetBuffStateSignal getBuffStateSignal { get; set; }

		[Inject]
		public StartCurrencyBuffSignal startCurrencyBuffSignal { get; set; }

		[Inject]
		public StopCurrencyBuffSignal stopCurrencyBuffSignal { get; set; }

		[Inject]
		public IPartyFavorAnimationService partyFavorAnimationService { get; set; }

		[Inject]
		public ShowBuffInfoPopupSignal showBuffPopupSignal { get; set; }

		[Inject]
		public BuffInfoPopupClosedSignal buffPopupClosedSignal { get; set; }

		public override void OnRegister()
		{
			view.Init(localService);
			updateTimes = 15;
			ticketClickedSignal.AddListener(GetTicketClicked);
			ticketDeletedSignal.AddListener(ClearTicket);
			setFTUETextSignal.AddListener(SetFTUEText);
			slotFullSignal.AddListener(SetSlotFullText);
			startFillingPrestigeBarSignal.AddListener(StartFillingPrestige);
			pauseSignal.AddListener(OnPause);
			showBuffPopupSignal.AddListener(showBuffPopup);
			buffPopupClosedSignal.AddListener(hideBuffPopup);
			startCurrencyBuffSignal.AddListener(UpdateReward);
			stopCurrencyBuffSignal.AddListener(BuffEnded);
		}

		public override void OnRemove()
		{
			hideItemPopupSignal.Dispatch();
			view.ClearDummyObject();
			ticketClickedSignal.RemoveListener(GetTicketClicked);
			ticketDeletedSignal.RemoveListener(ClearTicket);
			setFTUETextSignal.RemoveListener(SetFTUEText);
			slotFullSignal.RemoveListener(SetSlotFullText);
			startFillingPrestigeBarSignal.RemoveListener(StartFillingPrestige);
			pauseSignal.RemoveListener(OnPause);
			startCurrencyBuffSignal.RemoveListener(UpdateReward);
			stopCurrencyBuffSignal.RemoveListener(BuffEnded);
			showBuffPopupSignal.RemoveListener(showBuffPopup);
			buffPopupClosedSignal.RemoveListener(hideBuffPopup);
			List<OrderBoardRequiredItemView> itemList = view.GetItemList();
			if (itemList != null)
			{
				foreach (OrderBoardRequiredItemView item in itemList)
				{
					if (item != null)
					{
						item.pointerUpSignal.RemoveListener(PointerUp);
						item.pointerDownSignal.RemoveListener(PointerDown);
					}
				}
			}
			if (fillBarRoutine != null && !completeFinished)
			{
				completeFinished = true;
				StopCoroutine(fillBarRoutine);
				fillOrderCallBack();
			}
		}

		private void ClearTicket()
		{
			view.TicketName.gameObject.SetActive(false);
			view.SetSlotFullText("NoTicketSelected");
			view.SetupItemCount(0);
			view.PrestigePanel.SetActive(false);
			view.OrderPanel.SetActive(true);
		}

		private void StartFillingPrestige(int targetBarValue, Action FillOrderCallback)
		{
			playSoundFXSignal.Dispatch("Play_prestige_bar_scale_01");
			fillOrderCallBack = FillOrderCallback;
			fillBarRoutine = FillProgreeBarThenCall(targetBarValue - currentPrestigePoints, FillOrderCallback);
			if (targetBarValue >= neededPrestigePoints)
			{
				closingModal = true;
			}
			StartCoroutine(fillBarRoutine);
		}

		private IEnumerator FillProgreeBarThenCall(int valueOffset, Action completeCallback)
		{
			float increment = (float)valueOffset / (float)updateTimes;
			float myPrestigePoints2 = 0f;
			view.GlowAnimation.SetActive(true);
			for (int i = 1; i <= updateTimes; i++)
			{
				if (!completeFinished)
				{
					myPrestigePoints2 = (float)currentPrestigePoints + (float)i * increment;
					view.SetPrestigeProgress(myPrestigePoints2, neededPrestigePoints);
					yield return new WaitForSeconds(0.05f);
				}
			}
			yield return new WaitForSeconds(0.25f);
			completeFinished = true;
			completeCallback();
			fillBarRoutine = null;
		}

		private void GetTicketClicked(OrderBoardTicket ticket, string title, bool mute)
		{
			ti = ticket.TransactionInst;
			int count = ti.Inputs.Count;
			if (!closingModal)
			{
				completeFinished = false;
			}
			view.SetupItemCount(count);
			UpdateReward();
			if (ticket.CharacterDefinitionId != 0)
			{
				SetupCharacterDetail(ticket.CharacterDefinitionId);
			}
			else
			{
				view.ClearDummyObject();
				view.SetPanelState(false);
				view.SetTitle(title);
				currentPrestigePoints = 0;
			}
			for (int i = 0; i < count; i++)
			{
				QuantityItem quantityItem = ti.Inputs[i];
				ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(quantityItem.ID);
				uint quantity = quantityItem.Quantity;
				Sprite icon = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
				if (string.IsNullOrEmpty(itemDefinition.Mask))
				{
					logger.Log(KampaiLogLevel.Error, "Your Item Definition: {0} doesn' have a mask image defined for the item icon: {1}", itemDefinition.ID, itemDefinition.Image);
					itemDefinition.Mask = "btn_Circle01_mask";
				}
				Sprite mask = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
				uint quantityByDefinitionId = playerService.GetQuantityByDefinitionId(quantityItem.ID);
				OrderBoardRequiredItemView orderBoardRequiredItemView = view.CreateRequiredItem(i, quantity, quantityByDefinitionId, icon, mask);
				orderBoardRequiredItemView.ItemDefinitionID = quantityItem.ID;
				orderBoardRequiredItemView.pointerUpSignal.AddListener(PointerUp);
				orderBoardRequiredItemView.pointerDownSignal.AddListener(PointerDown);
				if (partyFavorAnimationService.GetAllPartyFavorItems().Contains(itemDefinition.ID))
				{
					orderBoardRequiredItemView.IconAnimator.SetTrigger("IsPartyFavor");
				}
			}
		}

		private void BuffEnded()
		{
			view.DeactivateAllBuffVisuals();
		}

		private void UpdateReward()
		{
			getBuffStateSignal.Dispatch(BuffType.CURRENCY, SetReward);
		}

		private void SetReward(float modifier)
		{
			int xp = TransactionUtil.ExtractQuantityFromTransaction(ti, 2);
			int num = TransactionUtil.ExtractQuantityFromTransaction(ti, 0);
			int additionalBuffGrind = 0;
			bool flag = (int)(modifier * 100f) != 100;
			view.ActivateBuffIcons(flag, modifier);
			if (flag)
			{
				additionalBuffGrind = Mathf.CeilToInt((float)num * modifier - (float)num);
			}
			view.SetReward(num, xp, additionalBuffGrind);
		}

		private void SetupCharacterDetail(int characterDefID)
		{
			Prestige prestige = characterService.GetPrestige(characterDefID);
			currentPrestigePoints = prestige.CurrentPrestigePoints;
			neededPrestigePoints = prestige.NeededPrestigePoints;
			currentPrestigeLevel = prestige.CurrentPrestigeLevel;
			bool orderInstructionEnabled = false;
			PrestigeType type = prestige.Definition.Type;
			if ((type == PrestigeType.Minion && characterService.IsTikiBarFull()) || (type == PrestigeType.Villain && characterService.GetEmptyCabana() == null))
			{
				orderInstructionEnabled = true;
			}
			view.SetPanelState(true, currentPrestigeLevel, prestige, orderInstructionEnabled);
			view.SetPrestigeProgress(currentPrestigePoints, neededPrestigePoints);
			if (currentPrestigePoints > 0)
			{
				view.GlowAnimation.SetActive(true);
			}
			DummyCharacterType characterType = fancyUIService.GetCharacterType(characterDefID);
			DummyCharacterObject character = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.Happy, view.MinionSlot.transform, view.MinionSlot.VillainScale, view.MinionSlot.VillainPositionOffset, characterDefID);
			view.SetCharacter(character);
			moveAudioListener.Dispatch(false, view.MinionSlot.transform);
		}

		private void SetSlotFullText(string locKey)
		{
			view.SetSlotFullText(locKey);
			view.SetBuffRewardsPanelGlow(false);
		}

		private void SetFTUEText(string title)
		{
			string @string = localService.GetString(title);
			view.SetFTUEText(@string);
		}

		private void PointerDown(OrderBoardRequiredItemView itemView, RectTransform rectTransform)
		{
			int itemDefinitionID = itemView.ItemDefinitionID;
			showGoto = !itemView.playerHasEnoughItems;
			if (PointerDownWait != null)
			{
				StopCoroutine(PointerDownWait);
				PointerDownWait = null;
			}
			displayItemPopupSignal.Dispatch(itemDefinitionID, rectTransform, showGoto ? UIPopupType.GENERICGOTO : UIPopupType.GENERIC);
		}

		private void PointerUp()
		{
			if (PointerDownWait == null)
			{
				PointerDownWait = WaitASecond();
				StartCoroutine(PointerDownWait);
			}
		}

		private IEnumerator WaitASecond()
		{
			yield return new WaitForSeconds((!showGoto) ? 0.5f : 1f);
			hideItemPopupSignal.Dispatch();
		}

		private void OnPause()
		{
			hideItemPopupSignal.Dispatch();
		}

		private void showBuffPopup(Vector3 vector, float offset)
		{
			view.toggleMinionSlot(false);
		}

		private void hideBuffPopup()
		{
			StartCoroutine(WaitForPopupClose());
		}

		private IEnumerator WaitForPopupClose()
		{
			yield return new WaitForSeconds(0.23f);
			if (view != null)
			{
				view.toggleMinionSlot(true);
			}
		}
	}
}

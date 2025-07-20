using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SocialPartyFillOrderButtonMediator : KampaiMediator
	{
		public class SocialPartyFillOrderButtonMediatorData
		{
			public SocialTeam team;

			public SocialOrderProgress progress;

			public SocialEventOrderDefinition orderDefintion;

			public GameObject parent;

			public TransactionDefinition orderTransactionDefinition;

			public int index;

			public Vector3 centerPos;

			public Signal<bool> fillOrderSignal;

			public bool autoFill;
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("SocialPartyFillOrderButtonMediator") as IKampaiLogger;

		private SocialPartyFillOrderButtonMediatorData data;

		private TransactionDefinition orderTransactionDefinition;

		private uint playerItemQuantity;

		private uint requiredItemQuantity;

		private int index;

		private bool autoFilled;

		public float ButtonWidth = 0.32f;

		public float ButtonHeight = 0.32f;

		[Inject]
		public SocialPartyFillOrderButtonView view { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ShowSocialPartyRewardSignal socialPartyRewardSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public SocialPartyFillOrderButtonMediatorUpdateSignal socialPartyFillOrderButtonMediatorUpdateSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public SocialPartyFillOrderSetupUISignal socialPartyFillOrderSetupUISignal { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageCapacitySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public CheckIfShouldStartPartySignal checkIfShouldStartPartySignal { get; set; }

		[Inject]
		public SocialPartyFillOrderCompleteSignal socialPartyFillOrderCompleteSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			data = args.Get<SocialPartyFillOrderButtonMediatorData>();
			if (data.parent == null)
			{
				Object.Destroy(view.gameObject);
				return;
			}
			index = data.index;
			int num = index / 3;
			int num2 = num;
			int num3 = index % 3;
			RectTransform rectTransform = view.transform as RectTransform;
			rectTransform.SetParent(data.parent.transform);
			rectTransform.localPosition = Vector3.zero;
			rectTransform.localScale = Vector3.one;
			rectTransform.offsetMin = new Vector2(0f, 0f);
			rectTransform.offsetMax = new Vector2(0f, 0f);
			rectTransform.anchorMin = new Vector2((0.5f - 0.5f * ButtonWidth) * (float)num3, 1f + 0.5f * (ButtonHeight - 1f) * (float)num2 - ButtonHeight);
			rectTransform.anchorMax = new Vector2((0.5f - 0.5f * ButtonWidth) * (float)num3 + ButtonWidth, 1f + 0.5f * (ButtonHeight - 1f) * (float)num2);
			view.fillOrderButton.Init();
			SetupOrder();
			DisplayableDefinition displayableDefinition = definitionService.Get(orderTransactionDefinition.Inputs[0].ID) as DisplayableDefinition;
			view.CreatFillOrderPopupIndicator(displayableDefinition.Image, displayableDefinition.Mask);
			data.fillOrderSignal.AddListener(view.SetButtonInteractable);
		}

		public override void OnRegister()
		{
			socialPartyFillOrderButtonMediatorUpdateSignal.AddListener(UpdateDetails);
			view.fillOrderButton.ClickedSignal.AddListener(FillOrderButton);
		}

		public override void OnRemove()
		{
			view.fillOrderButton.ClickedSignal.RemoveListener(FillOrderButton);
			socialPartyFillOrderButtonMediatorUpdateSignal.RemoveListener(UpdateDetails);
			data.fillOrderSignal.RemoveListener(view.SetButtonInteractable);
		}

		public void UpdateDetails(SocialPartyFillOrderButtonMediatorData details)
		{
			if (data != null && details.index == data.index)
			{
				data = details;
				SetupOrder();
			}
		}

		public void SetupOrder()
		{
			orderTransactionDefinition = definitionService.Get(data.orderDefintion.Transaction) as TransactionDefinition;
			data.orderTransactionDefinition = orderTransactionDefinition;
			if (data.progress == null || data.progress.CompletedByUserId == null)
			{
				SetupIncompleteOrder();
			}
			else
			{
				SetupCompleteOrder();
			}
		}

		private void SetupIncompleteOrder()
		{
			view.orderOpenPanel.SetActive(true);
			view.orderClosedPanel.SetActive(false);
			playerItemQuantity = playerService.GetQuantityByDefinitionId(orderTransactionDefinition.Inputs[0].ID);
			requiredItemQuantity = orderTransactionDefinition.Inputs[0].Quantity;
			if (playerService.IsMinionPartyUnlocked())
			{
				view.xpIcon.SetActive(false);
				view.funIcon.SetActive(true);
			}
			else
			{
				view.xpIcon.SetActive(true);
				view.funIcon.SetActive(false);
			}
			view.orderOpenTextAmount.text = string.Format("{0}/{1}", playerItemQuantity, requiredItemQuantity);
			foreach (QuantityItem output in orderTransactionDefinition.Outputs)
			{
				if (output.ID == 0)
				{
					view.grindReward.text = UIUtils.FormatLargeNumber((int)output.Quantity);
				}
				else if (output.ID == 2)
				{
					view.xpReward.text = UIUtils.FormatLargeNumber((int)output.Quantity);
				}
			}
			if (playerItemQuantity == requiredItemQuantity)
			{
				view.fillOrderButton.SetFillOrderButtonState(OrderBoardButtonState.MeetRequirement);
				return;
			}
			view.missingItems = playerService.GetMissingItemListFromTransaction(orderTransactionDefinition);
			int rushCost = playerService.CalculateRushCost(view.missingItems);
			view.fillOrderButton.SetFillOrderButtonState(OrderBoardButtonState.Rush, rushCost);
		}

		private void SetupCompleteOrder()
		{
			view.orderOpenPanel.SetActive(false);
			view.orderClosedPanel.SetActive(true);
			view.orderClosedImagePicture.gameObject.SetActive(false);
			view.orderClosedNoLoginImageCheck.gameObject.SetActive(true);
			FacebookService facebookService = this.facebookService as FacebookService;
			if (this.facebookService.isLoggedIn)
			{
				foreach (UserIdentity member in data.team.Members)
				{
					if (member.UserID == data.progress.CompletedByUserId)
					{
						string externalID = member.ExternalID;
						if (facebookService.userPictures.ContainsKey(externalID))
						{
							OnFacebookPictureComplete(externalID);
						}
						else
						{
							Signal<string> signal = new Signal<string>();
							signal.AddListener(OnFacebookPictureComplete);
							StartCoroutine(facebookService.DownloadUserPicture(externalID, signal));
						}
					}
				}
				return;
			}
			view.orderClosedImagePicture.gameObject.SetActive(false);
			view.orderClosedNoLoginImageCheck.gameObject.SetActive(true);
		}

		private void FillOrderButton()
		{
			if (view.fillOrderButton.isDoubleConfirmed() && view.fillOrderButton.enabled)
			{
				if (view.fillOrderButton.GetLastRushCost() > 0)
				{
					soundFXSignal.Dispatch("Play_button_premium_01");
					playerService.ProcessRush(view.fillOrderButton.GetLastRushCost(), view.missingItems, true, "Concert_Rush", RushItemCallBack, true);
					view.fillOrderButton.GetComponent<Button>().interactable = false;
				}
				else
				{
					soundFXSignal.Dispatch("Play_button_click_01");
					FinshButtonClick();
					data.fillOrderSignal.Dispatch(false);
				}
			}
		}

		private void RushItemCallBack(PendingCurrencyTransaction pendingTransaction)
		{
			if (pendingTransaction.Success)
			{
				setPremiumCurrencySignal.Dispatch();
				FinshButtonClick();
			}
			else if (view != null && view.fillOrderButton != null)
			{
				view.fillOrderButton.GetComponent<Button>().interactable = true;
			}
		}

		private void FinshButtonClick()
		{
			Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
			signal.AddListener(FillOrderResponse);
			timedSocialEventService.FillOrder(data.team.SocialEventId, data.team.ID, data.orderDefintion.OrderID, signal);
			PlayTweens();
		}

		private void FillOrderResponse(SocialTeamResponse response, ErrorResponse error)
		{
			data.fillOrderSignal.Dispatch(true);
			timedSocialEventService.setRewardCutscene(true);
			if (response != null && response.Error != null && response.Error.Type == SocialEventError.ErrorType.ORDER_ALREADY_FILLED)
			{
				FillOrderComplete(response, null);
			}
			else if (response != null && response.Error == null)
			{
				playerService.RunEntireTransaction(orderTransactionDefinition, TransactionTarget.NO_VISUAL, delegate(PendingCurrencyTransaction pct)
				{
					FillOrderComplete(response, pct);
				});
			}
			else
			{
				networkConnectionLostSignal.Dispatch();
				timedSocialEventService.setRewardCutscene(false);
				checkIfShouldStartPartySignal.Dispatch();
			}
		}

		private void FillOrderComplete(SocialTeamResponse response, PendingCurrencyTransaction pct)
		{
			if (pct != null && !pct.Success)
			{
				logger.Warning("Fill Order transaction failed.");
			}
			else
			{
				ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(orderTransactionDefinition.Inputs[0].ID);
				int xPOutputForTransaction = TransactionUtil.GetXPOutputForTransaction(orderTransactionDefinition);
				telemetryService.Send_Telemetry_EVT_SOCIAL_EVENT_CONTRIBUTION(itemDefinition.Description, (int)orderTransactionDefinition.Inputs[0].Quantity, response.Team.Members.Count, xPOutputForTransaction);
			}
			setPremiumCurrencySignal.Dispatch();
			setStorageCapacitySignal.Dispatch();
			if (!response.UserEvent.RewardClaimed && response.Team.OrderProgress.Count == timedSocialEventService.GetCurrentSocialEvent().Orders.Count)
			{
				hideSignal.Dispatch("SocialSkrim");
				guiService.Execute(GUIOperation.Unload, "SocialPartyFillOrderScreen");
				socialPartyRewardSignal.Dispatch(timedSocialEventService.GetCurrentSocialEvent().ID);
			}
			else
			{
				socialPartyFillOrderSetupUISignal.Dispatch(response.Team);
				timedSocialEventService.setRewardCutscene(false);
				checkIfShouldStartPartySignal.Dispatch();
				socialPartyFillOrderCompleteSignal.Dispatch();
			}
		}

		private void PlayTweens()
		{
			tweenSignal.Dispatch(uiCamera.WorldToScreenPoint(view.grindImage.transform.position), DestinationType.GRIND, 0, false);
			tweenSignal.Dispatch(uiCamera.WorldToScreenPoint(view.xpImage.transform.position), DestinationType.XP, 2, false);
		}

		private void OnFacebookPictureComplete(string id)
		{
			FacebookService facebookService = this.facebookService as FacebookService;
			Texture userPicture = facebookService.GetUserPicture(id);
			if (userPicture != null)
			{
				view.orderClosedNoLoginImageCheck.gameObject.SetActive(false);
				view.orderClosedImagePicture.gameObject.SetActive(true);
				Sprite sprite = Sprite.Create(userPicture as Texture2D, new Rect(0f, 0f, userPicture.width, userPicture.height), new Vector2(0f, 0f));
				view.orderClosedImagePicture.sprite = sprite;
			}
			else
			{
				view.orderClosedNoLoginImageCheck.gameObject.SetActive(true);
				view.orderClosedImagePicture.gameObject.SetActive(false);
				logger.Warning("OnFacebookPictureComplete texture is null for Facebook ID: {0}", id);
			}
		}

		protected override void Update()
		{
			if (data != null && data.autoFill && !autoFilled)
			{
				autoFilled = true;
				FillOrderButton();
			}
		}
	}
}

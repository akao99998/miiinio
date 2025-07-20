using System;
using System.Collections;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class XPBarFunMediator : EventMediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("XPBarFunMediator") as IKampaiLogger;

		private MinionParty minionParty;

		private int _currentLevel = -1;

		private int _currentLevelTotalPoints;

		private int _prePartyLevel;

		private EndMinionPartySignal endMinionPartySignal;

		private ConfirmStartNewMinionPartySignal confirmStartParty;

		private LevelFunTable levelFunList;

		private bool partyHasStarted;

		private IEnumerator m_partyStartDelay;

		[Inject]
		public XPBarFunView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetLevelSignal setLevelSignal { get; set; }

		[Inject]
		public SetXPSignal setXPSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public XPFTUEHighlightSignal ftueHighlightSignal { get; set; }

		[Inject]
		public FireXPVFXSignal fireXpSignal { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public FTUELevelChangedSignal ftueLevelChangedSignal { get; set; }

		[Inject]
		public IPartyService partyService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public CheckIfShouldStartPartySignal checkIfShouldStartPartySignal { get; set; }

		[Inject]
		public DisableLeisureRushButtonSignal disableLeisureRushButtonSignal { get; set; }

		[Inject]
		public ExitVillainLairSignal exitVillainLairSignal { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		public override void OnRegister()
		{
			view.Init(positionService);
			setLevelSignal.AddListener(SetLevel);
			setXPSignal.AddListener(SetXP);
			ftueHighlightSignal.AddListener(ShowFTUEXP);
			fireXpSignal.AddListener(PlayXPVFX);
			ftueLevelChangedSignal.AddListener(OnFTUELevelChanged);
			checkIfShouldStartPartySignal.AddListener(CheckIfShouldStartParty);
			endMinionPartySignal = gameContext.injectionBinder.GetInstance<EndMinionPartySignal>();
			confirmStartParty = gameContext.injectionBinder.GetInstance<ConfirmStartNewMinionPartySignal>();
			endMinionPartySignal.AddListener(PartyOver);
			view.animateXP.AddListener(SetXP);
			minionParty = playerService.GetMinionPartyInstance();
			levelFunList = definitionService.Get<LevelFunTable>(1000009681);
			SetLevel();
			GlowOnStartup();
		}

		private void GlowOnStartup()
		{
			view.PlayInitialVFX();
		}

		public override void OnRemove()
		{
			setLevelSignal.RemoveListener(SetLevel);
			view.animateXP.RemoveListener(SetXP);
			setXPSignal.RemoveListener(SetXP);
			ftueHighlightSignal.RemoveListener(ShowFTUEXP);
			fireXpSignal.RemoveListener(PlayXPVFX);
			checkIfShouldStartPartySignal.RemoveListener(CheckIfShouldStartParty);
			ftueLevelChangedSignal.RemoveListener(OnFTUELevelChanged);
			endMinionPartySignal.RemoveListener(PartyOver);
		}

		private void OnFTUELevelChanged()
		{
			SetXP();
		}

		private void PartyOver(bool ignore)
		{
			logger.Log(KampaiLogLevel.Info, "Releasing xp bar to check for minion party being ready again (received callback from party)");
			playerService.UpdateMinionPartyPointValues();
			partyHasStarted = false;
			if (_prePartyLevel != _currentLevel)
			{
				view.ClearBar();
			}
			view.ClearAnnouncement();
			SetXP();
			m_partyStartDelay = null;
		}

		internal void SetLevel()
		{
			SetXP();
		}

		internal void SetXP()
		{
			if (!view.expTweenAudio)
			{
				playSFXSignal.Dispatch("Play_bar_scale_01");
				view.expTweenAudio = true;
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			if (_currentLevel != quantity)
			{
				_currentLevel = quantity;
				_currentLevelTotalPoints = partyService.GetCumulativePointsRequiredThisLevel(_currentLevel);
				view.SetLevel(levelFunList.partiesNeededList[quantity].PointsNeeded, quantity);
			}
			view.SetXP((uint)partyService.GetCumulativePointsEarnedThisLevel(quantity, minionParty.CurrentPartyIndex, (int)minionParty.CurrentPartyPoints), (uint)_currentLevelTotalPoints);
			if (minionParty.IsPartyReady)
			{
				CheckIfShouldStartParty();
			}
		}

		internal void ShowFTUEXP(bool show)
		{
			view.ShowFTUEXP(show);
		}

		private void PlayXPVFX()
		{
			view.PlayXPVFX();
		}

		private void CheckIfShouldStartParty()
		{
			if (minionParty.IsPartyReady && !partyHasStarted && !timedSocialEventService.isRewardCutscene())
			{
				BeginParty();
				return;
			}
			if (partyHasStarted)
			{
				logger.Log(KampaiLogLevel.Info, "In XPBarFunMediator.CheckIfShouldStartParty: blocking start of party because partyHasStarted = true.");
			}
			if (timedSocialEventService.isRewardCutscene())
			{
				logger.Log(KampaiLogLevel.Info, "In XPBarFunMediator.CheckIfShouldStartParty: blocking start of party because because timedSocialEventService.isRewardCutscene() = true.");
			}
		}

		internal IEnumerator StartNewPartyWithDelay()
		{
			yield return new WaitForSeconds(1.5f);
			if (!playerService.GetMinionPartyInstance().CharacterUnlocking)
			{
				ConfirmStartNewParty();
			}
		}

		private void ConfirmStartNewParty()
		{
			logger.Log(KampaiLogLevel.Info, "Locking xp bar mediator from sending party signal: will release upon callback from party ending.  Level={0} and index={1} ", _currentLevel, minionParty.CurrentPartyIndex);
			confirmStartParty.Dispatch(true);
		}

		private void BeginParty()
		{
			partyHasStarted = true;
			disableLeisureRushButtonSignal.Dispatch();
			view.SetAnnouncementText(localService.GetString("FunbarPartyPrompt"));
			m_partyStartDelay = StartNewPartyWithDelay();
			_prePartyLevel = _currentLevel;
			if (villainLairModel.currentActiveLair == null)
			{
				StartCoroutine(m_partyStartDelay);
				return;
			}
			exitVillainLairSignal.Dispatch(new Boxed<Action>(delegate
			{
				StartCoroutine(m_partyStartDelay);
			}));
		}
	}
}

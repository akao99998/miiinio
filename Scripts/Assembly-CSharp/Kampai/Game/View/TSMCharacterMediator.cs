using System;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class TSMCharacterMediator : Mediator
	{
		private NamedCharacterManagerView namedCharacterManagerView;

		private TSMCharacterHideState hideState;

		private bool isInParty;

		private bool isChestIntro;

		[Inject]
		public TSMCharacterView View { get; set; }

		[Inject]
		public HideTSMCharacterSignal HideTSMCharacterSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PostMinionPartyStartSignal minionPartyStartSignal { get; set; }

		[Inject]
		public PostMinionPartyEndSignal minionPartyEndSignal { get; set; }

		[Inject]
		public CheckTriggersSignal checkTriggersSignal { get; set; }

		[Inject]
		public TSMReachedDestinationSignal tsmReachedDestinationSignal { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject NamedCharacterManager { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSFX { get; set; }

		[Inject]
		public CaptainWaveAndCallCallbackSignal waveAndCallbackSignal { get; set; }

		[Inject]
		public TSMStartIntroAnimation tsmStartIntroAnimation { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public CheckTriggersSignal triggersSignal { get; set; }

		[Inject]
		public TSMTreasureCollectedSignal treasureCollectedSignal { get; set; }

		public override void OnRegister()
		{
			namedCharacterManagerView = NamedCharacterManager.GetComponent<NamedCharacterManagerView>();
			HideTSMCharacterSignal.AddListener(HideTSMCharacter);
			View.RemoveCharacterSignal.AddListener(RemoveTSMCharacter);
			minionPartyStartSignal.AddListener(OnPartyStart);
			minionPartyEndSignal.AddListener(OnPartyEnd);
			View.NextPartyAnimSignal.AddListener(PlayPartyAnimation);
			View.DestinationReachedSignal.AddListener(DestinationReached);
			waveAndCallbackSignal.AddListener(WaveAndCallback);
			tsmStartIntroAnimation.AddListener(StartIntro);
			View.ChestReadyToOpen.AddListener(ChestReadyToOpen);
			treasureCollectedSignal.AddListener(TreasureCollected);
			Init();
		}

		public override void OnRemove()
		{
			treasureCollectedSignal.RemoveListener(TreasureCollected);
			HideTSMCharacterSignal.RemoveListener(HideTSMCharacter);
			View.RemoveCharacterSignal.RemoveListener(RemoveTSMCharacter);
			minionPartyStartSignal.RemoveListener(OnPartyStart);
			minionPartyEndSignal.RemoveListener(OnPartyEnd);
			View.NextPartyAnimSignal.RemoveListener(PlayPartyAnimation);
			View.DestinationReachedSignal.RemoveListener(DestinationReached);
			waveAndCallbackSignal.RemoveListener(WaveAndCallback);
			tsmStartIntroAnimation.RemoveListener(StartIntro);
			View.ChestReadyToOpen.RemoveListener(ChestReadyToOpen);
		}

		private void Init()
		{
			playGlobalSFX.Dispatch("Play_tsm_arrive_01");
			View.ShowTSMCharacter();
		}

		private void HideTSMCharacter(TSMCharacterHideState hideState)
		{
			this.hideState = hideState;
			View.HideTSMCharacter(hideState);
		}

		private void DestinationReached()
		{
			tsmReachedDestinationSignal.Dispatch();
		}

		private void RemoveTSMCharacter()
		{
			UnityEngine.Object.Destroy(base.gameObject);
			namedCharacterManagerView.Remove(301);
			if (hideState == TSMCharacterHideState.CelebrateAndReturn)
			{
				checkTriggersSignal.Dispatch(301);
			}
		}

		private void OnPartyStart()
		{
			isInParty = true;
			PlayPartyAnimation();
		}

		private void OnPartyEnd()
		{
			isInParty = false;
		}

		private void PlayPartyAnimation()
		{
			if (isInParty)
			{
				int partyAnimationId = definitionService.Get<TSMCharacterDefinition>(View.DefinitionID).PartyAnimationId;
				WeightedInstance weightedInstance = playerService.GetWeightedInstance(partyAnimationId);
				MinionAnimationDefinition minionAnimationDefinition = definitionService.Get<MinionAnimationDefinition>(weightedInstance.NextPick(randomService).ID);
				if (minionAnimationDefinition != null)
				{
					View.PlayPartyAnimation(minionAnimationDefinition);
				}
			}
			else if (isChestIntro)
			{
				View.ChestIntroPostParty();
			}
			else
			{
				RuntimeAnimatorController animController = KampaiResources.Load<RuntimeAnimatorController>("asm_unique_sales_minion_intro");
				View.SetAnimController(animController);
			}
		}

		private void StartIntro(bool isChestIntro)
		{
			this.isChestIntro = isChestIntro;
			if (isChestIntro)
			{
				View.StartChestIntro();
			}
			else
			{
				View.ShowTSMCharacter();
			}
		}

		private void WaveAndCallback(Action callback, bool isChestIntro)
		{
			this.isChestIntro = isChestIntro;
			if (!isChestIntro)
			{
				View.SayCheese(callback);
			}
			else
			{
				View.OpenChest(callback);
			}
		}

		private void ChestReadyToOpen()
		{
			tsmReachedDestinationSignal.Dispatch();
		}

		private void TreasureCollected()
		{
			HideTSMCharacter(TSMCharacterHideState.Hide);
			TSMCharacterDefinition tSMCharacterDefinition = definitionService.Get<TSMCharacterDefinition>();
			timeEventService.AddEvent(View.ID, timeService.CurrentTime(), tSMCharacterDefinition.CooldownInSeconds, triggersSignal);
		}
	}
}

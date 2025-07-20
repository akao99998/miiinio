using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class BuddyWelcomePanelMediator : KampaiMediator
	{
		private CharacterWelcomeState currentState;

		private Prestige prestige;

		[Inject]
		public BuddyWelcomePanelView view { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public FTUECloseBuddySignal closeBuddySignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CameraAutoMoveToPositionSignal cameraAutoMoveToPositionSignal { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.OnMenuClose.AddListener(OnMenuClose);
			view.SetUpInjections(positionService);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			prestige = args.Get<Prestige>();
			CharacterWelcomeState state = args.Get<CharacterWelcomeState>();
			Load(state);
			if (prestige.CurrentPrestigeLevel >= 1)
			{
				hideAllWayFindersSignal.Dispatch();
			}
			StartCoroutine(AnimSequence(state));
		}

		private IEnumerator AnimSequence(CharacterWelcomeState state)
		{
			yield return new WaitForSeconds(0.2f);
			PrestigeType prestigeType = prestige.Definition.Type;
			if ((prestigeType == PrestigeType.Minion && state == CharacterWelcomeState.Welcome && (prestige.Definition.PrestigeLevelSettings == null || prestige.CurrentPrestigeLevel >= 0)) || (prestigeType == PrestigeType.Minion && state == CharacterWelcomeState.Farewell) || (prestigeType == PrestigeType.Villain && state == CharacterWelcomeState.Welcome))
			{
				if (prestige.CurrentPrestigeLevel <= 0 && state == CharacterWelcomeState.Welcome)
				{
					if (prestigeType == PrestigeType.Villain)
					{
						cameraAutoMoveToPositionSignal.Dispatch(GameConstants.Building.WELCOME_VILLAIN_LOCATION, 0.97f, false);
					}
					else
					{
						cameraAutoMoveToPositionSignal.Dispatch(GameConstants.Building.WELCOME_NAMED_CHARACTER_LOCATION, 0.97f, false);
					}
				}
				else
				{
					cameraAutoMoveToPositionSignal.Dispatch(GameConstants.Building.TIKI_BAR_PAN_LOCATION, 0.97f, false);
				}
			}
			ActionableObject actionableObject = ActionableObjectManagerView.GetFromAllObjects(prestige.trackedInstanceId);
			view.SetUpCharacterObject(actionableObject as CharacterObject);
			if (currentState == CharacterWelcomeState.Welcome)
			{
				gameContext.injectionBinder.GetInstance<BuildingReactionSignal>().Dispatch(new Boxed<Vector3>(actionableObject.transform.position), false);
			}
			yield return new WaitForSeconds(0.8f);
			view.Initialized = true;
			view.Open();
			yield return new WaitForSeconds(view.FadeOutTime);
			Close();
		}

		private void OnMenuClose()
		{
			closeBuddySignal.Dispatch();
			guiService.Execute(GUIOperation.Unload, "popup_CharacterState");
			uiModel.WelcomeBuddyOpen = false;
		}

		private void Close()
		{
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			view.Close();
			if (prestige.CurrentPrestigeLevel >= 1)
			{
				showAllWayFindersSignal.Dispatch();
			}
			pickControllerModel.SetIgnoreInstance(313, false);
		}

		public void Load(CharacterWelcomeState state)
		{
			currentState = state;
			PlayAudio();
			PrestigeDefinition prestigeDefinition = ((prestige != null) ? prestige.Definition : null);
			switch (state)
			{
			case CharacterWelcomeState.Farewell:
				view.Init("FarewellTitle", prestigeDefinition.LocalizedKey);
				break;
			case CharacterWelcomeState.Welcome:
				view.Init((prestige.CurrentPrestigeLevel <= 0) ? "WelcomeTitle" : "RePrestigeTitle", prestigeDefinition.LocalizedKey);
				break;
			}
		}

		private void PlayAudio()
		{
			PrestigeType type = prestige.Definition.Type;
			if (currentState == CharacterWelcomeState.Welcome && type == PrestigeType.Minion)
			{
				playSFXSignal.Dispatch("Play_minionArrives_01");
			}
			else if (currentState == CharacterWelcomeState.Farewell && type == PrestigeType.Minion)
			{
				playSFXSignal.Dispatch("Play_minionUnlock_01");
			}
			else if (currentState == CharacterWelcomeState.Welcome && type == PrestigeType.Villain)
			{
				playSFXSignal.Dispatch("Play_villainArrives_01");
			}
			else if (currentState == CharacterWelcomeState.Farewell && type == PrestigeType.Villain)
			{
				playSFXSignal.Dispatch("Play_villainLeaves_01");
			}
		}
	}
}

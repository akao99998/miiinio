using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.Game.Mignette.View
{
	public abstract class MignetteManagerMediator<T> : Mediator where T : MignetteManagerView
	{
		private bool hasExited;

		private bool hasRequestedExit;

		private bool hasInitialized;

		private MignetteBuildingViewObject mignetteBuildingViewObject;

		[Inject]
		public T view { get; set; }

		[Inject]
		public IMignetteService mignetteService { get; set; }

		[Inject]
		public RequestStopMignetteSignal requestStopMignetteSignal { get; set; }

		[Inject]
		public MignetteEndedSignal mignetteEndedSignal { get; set; }

		[Inject]
		public StopMignetteSignal stopMignetteSignal { get; set; }

		[Inject]
		public PlayGlobalMusicSignal musicSignal { get; set; }

		[Inject]
		public PlayMignetteMusicSignal mignetteMusicSignal { get; set; }

		[Inject]
		public ShowAndIncreaseMignetteScoreSignal showResultsSignal { get; set; }

		[Inject]
		public EjectAllMinionsFromBuildingSignal ejectAllMinionsFromBuildingSignal { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public ScheduleCooldownSignal scheduleCooldownSignal { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public TemporaryMinionsService temporaryMinionsService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public abstract string MusicEventName { get; }

		public override void OnRegister()
		{
			base.OnRegister();
			mignetteService.RegisterListener(OnPressHelper);
			requestStopMignetteSignal.AddListener(OnRequestStopMignette);
			mignetteEndedSignal.AddListener(OnMignetteExit);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			mignetteService.UnregisterListener(OnPressHelper);
			requestStopMignetteSignal.RemoveListener(OnRequestStopMignette);
			mignetteEndedSignal.RemoveListener(OnMignetteExit);
		}

		public virtual void Start()
		{
			mignetteMusicSignal.Dispatch(base.gameObject, MusicEventName, PlayMignetteMusicCommand.MusicEvent.Start);
		}

		public virtual void Update()
		{
			if (!hasInitialized)
			{
				hasInitialized = true;
				mignetteGameModel.UsesTimerHUD = false;
				mignetteGameModel.UsesCounterHUD = false;
				if (view.MignetteBuildingObject != null)
				{
					mignetteBuildingViewObject = view.MignetteBuildingObject.GetComponent<MignetteBuildingViewObject>();
					if (mignetteBuildingViewObject != null)
					{
						mignetteGameModel.UsesProgressHUD = mignetteBuildingViewObject.UsesProgressHUD;
						mignetteGameModel.UsesTimerHUD = mignetteBuildingViewObject.UsesTimerHUD;
						mignetteGameModel.UsesCounterHUD = mignetteBuildingViewObject.UsesCounterHUD;
					}
				}
			}
			if (!hasExited)
			{
				T val = view;
				if (val.IsPaused != networkModel.isConnectionLost)
				{
					OnPauseStateChanged(networkModel.isConnectionLost);
				}
				mignetteGameModel.TotalEventTime = view.TotalEventTime;
				mignetteGameModel.ElapsedTime = view.TimeElapsed;
				mignetteGameModel.TimeRemaining = view.TotalEventTime - view.TimeElapsed;
				mignetteGameModel.PercentCompleted = view.PercentCompleted;
			}
		}

		private void OnRequestStopMignette(bool showScore)
		{
			if (!hasRequestedExit)
			{
				hasRequestedExit = true;
				RequestStopMignette(showScore);
			}
		}

		protected virtual void RequestStopMignette(bool showScore)
		{
			OnMignetteExit(showScore);
		}

		private void RemoveTemporaryMinions(int taskableBuildingID)
		{
			TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(taskableBuildingID);
			BuildingManagerView component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerView>();
			IDictionary<int, MinionObject> temporaryMinions = temporaryMinionsService.getTemporaryMinions();
			foreach (KeyValuePair<int, MinionObject> item in temporaryMinions)
			{
				int key = item.Key;
				if (key <= -100)
				{
					component.UntrackMinion(taskableBuildingID, key, byInstanceId);
					byInstanceId.RemoveMinion(item.Key, timeService.CurrentTime());
					Object.Destroy(item.Value.gameObject);
					temporaryMinionsService.removeTemporaryMinion(key);
				}
			}
		}

		private void OnMignetteExit(bool showScore)
		{
			if (hasExited)
			{
				return;
			}
			hasExited = true;
			bool flag = playerService.HasPurchasedMinigamePack();
			if (!showScore && !flag)
			{
				ejectAllMinionsFromBuildingSignal.Dispatch(mignetteGameModel.BuildingId);
			}
			int iD = view.MignetteBuildingObject.ID;
			if (flag)
			{
				RemoveTemporaryMinions(iD);
			}
			else
			{
				buildingChangeStateSignal.Dispatch(iD, BuildingState.Cooldown);
				if (mignetteGameModel.TriggerCooldownOnComplete)
				{
					scheduleCooldownSignal.Dispatch(new Tuple<int, bool>(mignetteGameModel.BuildingId, true), true);
				}
			}
			if (showScore)
			{
				Dictionary<string, float> dictionary = new Dictionary<string, float>();
				dictionary.Add("Cue", 1f);
				Dictionary<string, float> type = dictionary;
				musicSignal.Dispatch("Play_mignetteTally_loop_01", type);
				showResultsSignal.Dispatch();
			}
			else
			{
				mignetteMusicSignal.Dispatch(base.gameObject, MusicEventName, PlayMignetteMusicCommand.MusicEvent.Stop);
				stopMignetteSignal.Dispatch(hasRequestedExit);
			}
		}

		private void OnPressHelper(Vector3 pos, int input, bool pressed)
		{
			T val = view;
			if (!val.IsPaused)
			{
				OnPress(pos, input, pressed);
			}
		}

		protected virtual void OnPress(Vector3 pos, int input, bool pressed)
		{
		}

		private void OnPauseStateChanged(bool isPaused)
		{
			T val = view;
			val.OnMignettePause(isPaused);
			OnMignettePause(isPaused);
		}

		protected virtual void OnMignettePause(bool isPaused)
		{
		}
	}
}

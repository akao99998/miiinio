using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class MinionManagerMediator : EventMediator
	{
		private bool animationsLoaded;

		private IKampaiLogger logger = LogManager.GetClassLogger("MinionManagerMediator") as IKampaiLogger;

		[Inject]
		public MinionManagerView view { get; set; }

		[Inject]
		public MinionMoveToSignal minionMoveToSignal { get; set; }

		[Inject]
		public AddMinionSignal addMinionSignal { get; set; }

		[Inject]
		public MinionWalkPathSignal minionWalkPathSignal { get; set; }

		[Inject]
		public MinionRunPathSignal minionRunPathSignal { get; set; }

		[Inject]
		public MinionAppearSignal minionAppearSignal { get; set; }

		[Inject]
		public AnimateSelectedMinionSignal animateSelectedMinionSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal stateChangeSignal { get; set; }

		[Inject]
		public StartMinionRouteSignal startMinionRouteSignal { get; set; }

		[Inject]
		public StartTeleportTaskSignal startTeleportTaskSignal { get; set; }

		[Inject]
		public StartTaskSignal startTaskSignal { get; set; }

		[Inject]
		public SignalActionSignal stopTaskSignal { get; set; }

		[Inject]
		public RelocateCharacterSignal relocateSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public StartGroupGachaSignal startGroupGachaSignal { get; set; }

		[Inject]
		public DeselectAllMinionsSignal deselectAllMinionsSignal { get; set; }

		[Inject]
		public StartIncidentalAnimationSignal startIncidentalAnimationSignal { get; set; }

		[Inject]
		public MinionAcknowledgeSignal minionAcknowledgeSignal { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		[Inject]
		public UpdateTaskedMinionSignal updateTaskedMinionSignal { get; set; }

		[Inject]
		public RestoreMinionStateSignal restoreMinionSignal { get; set; }

		[Inject]
		public MinionReactSignal reactSignal { get; set; }

		[Inject]
		public EnableMinionRendererSignal enableRendererSignal { get; set; }

		[Inject]
		public MoveMinionFinishedSignal moveMinionFinishedSignal { get; set; }

		[Inject]
		public PlayMinionNoAnimAudioSignal playMinionNoAnimAudioSignal { get; set; }

		[Inject]
		public AddMinionToTikiBarSignal addMinionTikiBarSignal { get; set; }

		[Inject]
		public MinionSeekPositionSignal minionSeekPositionSignal { get; set; }

		[Inject]
		public SetPartyStatesSignal setPartyStateSignal { get; set; }

		[Inject]
		public TapMinionSignal tapMinionSignal { get; set; }

		[Inject]
		public TeleportMinionsToTownForPartySignal teleportMinionsToTownSignal { get; set; }

		[Inject]
		public EndTownhallMinionPartyAnimationSignal endTownhallMinionPartyAnimationSignal { get; set; }

		[Inject]
		public ICoroutineProgressMonitor coroutineProgressMonitor { get; set; }

		[Inject]
		public MinionPartyAnimationSignal minionPartyAnimationSginal { get; set; }

		[Inject]
		public AllMinionLoadedSignal allMinionLoadedSignal { get; set; }

		[Inject]
		public StopMinionCampingSignal stopMinionCampingSignal { get; set; }

		[Inject]
		public IPartyFavorAnimationService partyFavorService { get; set; }

		[Inject]
		public AddMinionsPartyFavorSignal addMinionsToPartyFavorAnimSignal { get; set; }

		[Inject]
		public StartMinionPartyIntroSignal startMinionPartyIntroSignal { get; set; }

		[Inject]
		public StuartShowStartSignal stuartShowStartSignal { get; set; }

		[Inject]
		public StuartShowCompleteSignal stuartShowCompleteSignal { get; set; }

		[Inject]
		public AddSpecificMinionPartyFavorSignal addSpecificMinionSignal { get; set; }

		[Inject]
		public IncidentalPartyFavorAnimationCompletedSignal incidentalPartyFavorAnimationCompletedSignal { get; set; }

		public override void OnRegister()
		{
			view.Init();
			coroutineProgressMonitor.StartTask(CacheAnimations(), "cache minion anims");
			SetupSignals();
			SetupMoreSignals();
			MinionPartyDefinition minionPartyDefinition = definitionService.Get<MinionPartyDefinition>(80000);
			view.SetPartyLocation(new Boxed<Vector3>((Vector3)minionPartyDefinition.Center), minionPartyDefinition.PartyRadius);
		}

		private IEnumerator CacheAnimations()
		{
			yield return null;
			List<WeightedDefinition> weights = definitionService.GetAllGachaDefinitions();
			for (int j = 0; j < weights.Count; j++)
			{
				IList<WeightedQuantityItem> gachaChoices = weights[j].Entities;
				List<AnimationDefinition> gachas = new List<AnimationDefinition>(gachaChoices.Count);
				for (int k = 0; k < gachaChoices.Count; k++)
				{
					gachas.Add(definitionService.Get<GachaAnimationDefinition>(gachaChoices[k].ID));
				}
				yield return view.CacheAnimationsCoroutine(gachas);
			}
			List<MinionAnimationDefinition> anims = definitionService.GetAll<MinionAnimationDefinition>();
			yield return view.CacheAnimationsCoroutine(anims);
			List<UIAnimationDefinition> uiAnims = definitionService.GetAll<UIAnimationDefinition>();
			for (int i = 0; i < uiAnims.Count; i++)
			{
				UIAnimationDefinition anim = uiAnims[i];
				KampaiResources.Load(anim.AnimationClipName, typeof(AnimationClip));
				yield return null;
			}
			animationsLoaded = true;
		}

		private void SetupSignals()
		{
			view.idleMinionSignal.AddListener(IdleMinion);
			addMinionSignal.AddListener(AddMinion);
			minionWalkPathSignal.AddListener(WalkPath);
			minionRunPathSignal.AddListener(RunPath);
			startMinionRouteSignal.AddListener(StartMinionRoute);
			minionAppearSignal.AddListener(MinionAppear);
			animateSelectedMinionSignal.AddListener(SelectMinion);
			startGroupGachaSignal.AddListener(StartGroupGacha);
			startIncidentalAnimationSignal.AddListener(StartIncidentalAnimation);
			minionAcknowledgeSignal.AddListener(MinionAcknowledgement);
			updateTaskedMinionSignal.AddListener(UpdateTaskedMinion);
			reactSignal.AddListener(MinionReact);
			enableRendererSignal.AddListener(EnableMinionRenderer);
			startTeleportTaskSignal.AddListener(MinionTeleport);
			playMinionNoAnimAudioSignal.AddListener(PlayMinionAudio);
			minionSeekPositionSignal.AddListener(SeekPosition);
			setPartyStateSignal.AddListener(SetPartyStates);
			tapMinionSignal.AddListener(TapMinion);
			teleportMinionsToTownSignal.AddListener(TeleportMinionsToTown);
		}

		private void SetupMoreSignals()
		{
			endTownhallMinionPartyAnimationSignal.AddListener(EndTownhallMinionPartyAnimation);
			minionPartyAnimationSginal.AddListener(PlayPartyAnimation);
			allMinionLoadedSignal.AddListener(RestoreMinionParty);
			stopMinionCampingSignal.AddListener(StopMinionCamping);
			addMinionsToPartyFavorAnimSignal.AddListener(AddMinionsToPartyFavorAnimation);
			startMinionPartyIntroSignal.AddListener(ResetPartyAnimationCount);
			stuartShowStartSignal.AddListener(OnStuartConcertStart);
			stuartShowCompleteSignal.AddListener(OnStuartConcertEnd);
			incidentalPartyFavorAnimationCompletedSignal.AddListener(IncidentalPartyFavorComplete);
			addSpecificMinionSignal.AddListener(AddMinionToPartyFavorAnimation);
		}

		public override void OnRemove()
		{
			view.idleMinionSignal.RemoveListener(IdleMinion);
			addMinionSignal.RemoveListener(AddMinion);
			minionWalkPathSignal.RemoveListener(WalkPath);
			minionRunPathSignal.RemoveListener(RunPath);
			startMinionRouteSignal.RemoveListener(StartMinionRoute);
			minionAppearSignal.RemoveListener(MinionAppear);
			animateSelectedMinionSignal.RemoveListener(SelectMinion);
			startGroupGachaSignal.RemoveListener(StartGroupGacha);
			startIncidentalAnimationSignal.RemoveListener(StartIncidentalAnimation);
			minionAcknowledgeSignal.RemoveListener(MinionAcknowledgement);
			updateTaskedMinionSignal.RemoveListener(UpdateTaskedMinion);
			reactSignal.RemoveListener(MinionReact);
			enableRendererSignal.RemoveListener(EnableMinionRenderer);
			startTeleportTaskSignal.RemoveListener(MinionTeleport);
			playMinionNoAnimAudioSignal.RemoveListener(PlayMinionAudio);
			minionSeekPositionSignal.RemoveListener(SeekPosition);
			setPartyStateSignal.RemoveListener(SetPartyStates);
			tapMinionSignal.RemoveListener(TapMinion);
			teleportMinionsToTownSignal.RemoveListener(TeleportMinionsToTown);
			CleanupSignals();
		}

		private void CleanupSignals()
		{
			endTownhallMinionPartyAnimationSignal.RemoveListener(EndTownhallMinionPartyAnimation);
			minionPartyAnimationSginal.RemoveListener(PlayPartyAnimation);
			allMinionLoadedSignal.RemoveListener(RestoreMinionParty);
			stopMinionCampingSignal.RemoveListener(StopMinionCamping);
			addMinionsToPartyFavorAnimSignal.RemoveListener(AddMinionsToPartyFavorAnimation);
			startMinionPartyIntroSignal.RemoveListener(ResetPartyAnimationCount);
			stuartShowStartSignal.RemoveListener(OnStuartConcertStart);
			stuartShowCompleteSignal.RemoveListener(OnStuartConcertEnd);
			incidentalPartyFavorAnimationCompletedSignal.RemoveListener(IncidentalPartyFavorComplete);
			addSpecificMinionSignal.RemoveListener(AddMinionToPartyFavorAnimation);
		}

		private void AddMinionsToPartyFavorAnimation(int partyFavorId)
		{
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				Minion minion = instancesByType[i];
				MinionObject minionObject = view.GetMinionObject(minion);
				if (!(minionObject == null) && !minion.IsDoingPartyFavorAnimation && minion.IsInMinionParty)
				{
					minion.IsDoingPartyFavorAnimation = true;
					partyFavorService.AddMinionsToPartyFavor(partyFavorId, minionObject);
					break;
				}
			}
		}

		private void AddMinionToPartyFavorAnimation(int partyFavorId, int minionID)
		{
			MinionObject minionObjectByID = view.GetMinionObjectByID(minionID);
			partyFavorService.AddMinionsToPartyFavor(partyFavorId, minionObjectByID);
		}

		private void StartGroupGacha(MinionAnimationInstructions instructions)
		{
			HashSet<int> minionIds = instructions.MinionIds;
			int count = minionIds.Count;
			WeightedDefinition gachaWeightsForNumMinions = definitionService.GetGachaWeightsForNumMinions(count, instructions.Party);
			WeightedInstance weightedInstance = playerService.GetWeightedInstance(gachaWeightsForNumMinions.ID);
			QuantityItem quantityItem = weightedInstance.NextPick(randomService);
			if (quantityItem.ID > 0)
			{
				GachaAnimationDefinition gachaPick = definitionService.Get<GachaAnimationDefinition>(quantityItem.ID);
				view.StartGroupGacha(gachaPick, minionIds, instructions.Center.Value, pathFinder);
			}
			else
			{
				deselectAllMinionsSignal.Dispatch();
			}
		}

		private void AddMinion(MinionObject minionObj)
		{
			view.Add(minionObj);
			restoreMinionSignal.Dispatch(minionObj.ID);
		}

		private void WalkPath(int minionID, IList<Vector3> path, float speed, bool muteStatus)
		{
			view.StartPathing(minionID, path, speed, muteStatus, moveMinionFinishedSignal);
		}

		private void RunPath(int minionID, IList<Vector3> path, float timeout, bool muteStatus)
		{
			view.StartPathing(minionID, path, 4.5f, muteStatus, moveMinionFinishedSignal);
		}

		private void StartMinionRoute(RouteInstructions routing)
		{
			TaskableBuilding taskableBuilding = routing.TargetBuilding as TaskableBuilding;
			if (taskableBuilding == null)
			{
				logger.Error("Trying to task a minion to a no-taskable building.");
			}
			view.StartMinionTask(routing.minion, taskableBuilding, startTaskSignal, stopTaskSignal, relocateSignal, routing.Path, routing.Rotation);
		}

		private void PlayMinionAudio(int MinionID, string audioEvent)
		{
			view.playMinionAudio(MinionID, audioEvent);
		}

		private void EnableMinionRenderer(int minionID, bool enable)
		{
			view.EnableRenderer(minionID, enable);
		}

		private void MinionTeleport(Minion minion, TaskableBuilding building)
		{
			view.TeleportMinionTask(minion, building, startTaskSignal, stopTaskSignal, relocateSignal);
		}

		private void UpdateTaskedMinion(int minionID, MinionTaskInfo taskInfo)
		{
			view.UpdateTaskedMinion(minionID, taskInfo);
		}

		private void MinionAppear(int minionID, Vector3 pos)
		{
			view.MinionAppear(minionID, pos);
		}

		private void StopMinionCamping()
		{
			view.StopMinionCamping();
		}

		private void SelectMinion(SelectMinionState state)
		{
			GachaAnimationDefinition gachaAnimationDefinition = null;
			if (state.triggerIncidentalAnimation)
			{
				gachaAnimationDefinition = GetNextGacha(false);
			}
			MinionAnimationDefinition minionAnimDef = null;
			if (gachaAnimationDefinition != null)
			{
				minionAnimDef = definitionService.Get<MinionAnimationDefinition>(gachaAnimationDefinition.AnimationID);
			}
			view.SelectMinion(state.minionID, minionAnimDef, state.runLocation, minionMoveToSignal, state.muteStatus);
		}

		private void TapMinion(int minionID)
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionID);
			if (byInstanceId == null)
			{
				logger.Debug("MinionManagerMediator:TapMinion - KAMPAI 7668 minion was null");
			}
			else
			{
				if (byInstanceId.State != MinionState.Idle)
				{
					return;
				}
				GachaAnimationDefinition nextGacha = GetNextGacha(false);
				if (nextGacha == null)
				{
					logger.Debug("MinionManagerMediator:TapMinion - KAMPAI 7668 gacha was null");
					return;
				}
				MinionAnimationDefinition minionAnimationDefinition = definitionService.Get<MinionAnimationDefinition>(nextGacha.AnimationID);
				if (minionAnimationDefinition == null)
				{
					logger.Debug("MinionManagerMediator:TapMinion - KAMPAI 7668 minionAnimDef was null");
				}
				else
				{
					view.AnimateMinion(minionID, minionAnimationDefinition, false);
				}
			}
		}

		private GachaAnimationDefinition GetNextGacha(bool party)
		{
			WeightedInstance weightedInstance = playerService.GetWeightedInstance(definitionService.GetGachaWeightsForNumMinions(1, party).ID);
			return definitionService.Get<GachaAnimationDefinition>(weightedInstance.NextPick(randomService).ID);
		}

		public void IdleMinion(MinionObject minionObject)
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionObject.ID);
			MinionState state = byInstanceId.State;
			if (byInstanceId.HasPrestige)
			{
				if (!minionObject.IsSeatedInTikiBar)
				{
					Prestige byInstanceId2 = playerService.GetByInstanceId<Prestige>(byInstanceId.PrestigeId);
					IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<TikiBarBuildingDefinition>();
					if (byInstanceId2.state == PrestigeState.Questing && instancesByDefinition != null && instancesByDefinition.Count != 0)
					{
						TikiBarBuilding tikiBarBuilding = instancesByDefinition[0] as TikiBarBuilding;
						addMinionTikiBarSignal.Dispatch(tikiBarBuilding, byInstanceId, byInstanceId2, tikiBarBuilding.GetMinionSlotIndex(byInstanceId2.Definition.ID));
						return;
					}
				}
				if (state == MinionState.Questing)
				{
					return;
				}
			}
			if (state != MinionState.Selected && state != MinionState.Idle)
			{
				view.SetMinionMute(byInstanceId.ID, false);
				stateChangeSignal.Dispatch(byInstanceId.ID, MinionState.Idle);
				minionObject.Wander();
				return;
			}
			switch (state)
			{
			case MinionState.Idle:
				minionObject.Wander();
				break;
			case MinionState.Selected:
				view.SetMinionReady(byInstanceId.ID);
				break;
			}
		}

		private void StartIncidentalAnimation(int minionID, int animationDefinitionId)
		{
			view.StartMinionAnimation(minionID, definitionService.Get<MinionAnimationDefinition>(animationDefinitionId), true);
		}

		private void MinionAcknowledgement(int minionID, float rotateTo, int animationDefinitionId)
		{
			if (animationsLoaded)
			{
				view.MinionAcknowledgement(minionID, rotateTo, definitionService.Get<MinionAnimationDefinition>(animationDefinitionId));
			}
		}

		private void SeekPosition(int minionID, Vector3 pos, float threshold)
		{
			view.SeekPosition(minionID, pos, threshold);
		}

		private void MinionReact(ICollection<int> minionIds, Boxed<Vector3> buildingPos)
		{
			if (!animationsLoaded)
			{
				return;
			}
			WeightedInstance weightedInstance = playerService.GetWeightedInstance(4005);
			QuantityItem quantityItem = weightedInstance.NextPick(randomService);
			if (quantityItem.ID > 0)
			{
				GachaAnimationDefinition gachaAnimationDefinition = definitionService.Get<GachaAnimationDefinition>(quantityItem.ID);
				if (gachaAnimationDefinition == null)
				{
					logger.Log(KampaiLogLevel.Error, "Bad Gacha ID: {0}", quantityItem.ID);
				}
				else
				{
					view.MinionReact(gachaAnimationDefinition, minionIds, buildingPos);
				}
			}
		}

		private void SetPartyStates(bool gameIsStarting)
		{
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				if (instancesByType[i].State == MinionState.Tasking || instancesByType[i].State == MinionState.Leisure)
				{
					continue;
				}
				MinionAnimationDefinition partyStartAnimation = null;
				if (gameIsStarting)
				{
					MinionPartyDefinition minionPartyDefinition = definitionService.Get<MinionPartyDefinition>(80000);
					if (minionPartyDefinition.PartyAnimations > 0)
					{
						WeightedInstance weightedInstance = playerService.GetWeightedInstance(minionPartyDefinition.PartyAnimations);
						partyStartAnimation = definitionService.Get<MinionAnimationDefinition>(weightedInstance.NextPick(randomService).ID);
					}
				}
				view.SetPartyState(instancesByType[i].ID, instancesByType[i].Partying, gameIsStarting, partyStartAnimation);
			}
		}

		public void RestoreMinionParty()
		{
			MinionPartyDefinition definition = playerService.GetMinionPartyInstance().Definition;
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				if (item.IsInMinionParty)
				{
					view.Get(item.ID).EnterMinionParty((Vector3)definition.Center, definition.PartyRadius, definition.partyAnimationRestMin, definition.partyAnimationRestMax);
				}
			}
		}

		private void TeleportMinionsToTown()
		{
			pathFinder.ShuffleLists();
			MinionPartyDefinition definition = playerService.GetMinionPartyInstance().Definition;
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				item.IsInMinionParty = true;
				MinionObject minionObject = view.Get(item.ID);
				if (minionObject != null)
				{
					minionObject.EnterMinionParty((Vector3)definition.Center, definition.PartyRadius, definition.partyAnimationRestMin, definition.partyAnimationRestMax);
					if (item.State != MinionState.Tasking && item.State != MinionState.Questing && item.State != MinionState.Leisure && item.State != MinionState.PlayingMignette)
					{
						Vector3 pos = pathFinder.RandomPosition(item.IsInMinionParty);
						MinionAppear(item.ID, pos);
					}
				}
			}
		}

		private void OnStuartConcertStart()
		{
			StageBuildingDefinition stageBuildingDefinition = definitionService.Get<StageBuildingDefinition>(3054);
			Location location = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054).Location;
			Queue<int> minionListSortedByDistanceAndState = view.GetMinionListSortedByDistanceAndState(new Vector3(location.x, 0f, location.y));
			int num = 0;
			foreach (int item in minionListSortedByDistanceAndState)
			{
				MinionObject minionObject = view.Get(item);
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(item);
				if (byInstanceId.State != MinionState.Tasking && byInstanceId.State != MinionState.Questing && byInstanceId.State != MinionState.Leisure && byInstanceId.State != MinionState.PlayingMignette)
				{
					minionObject.ClearActionQueue();
					Vector3 stageBuildingPosition = pathFinder.GetStageBuildingPosition(num);
					MinionAppear(byInstanceId.ID, stageBuildingPosition);
					RuntimeAnimatorController controller = KampaiResources.Load<RuntimeAnimatorController>(stageBuildingDefinition.temporaryMinionASM);
					minionObject.EnqueueAction(new RotateAction(minionObject, Camera.main.transform.eulerAngles.y, 360f, logger));
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("randomizer", randomService.NextInt(stageBuildingDefinition.temporaryMinionAnimationCount));
					minionObject.EnqueueAction(new SetAnimatorAction(minionObject, controller, logger, dictionary));
					minionObject.EnqueueAction(new WaitForMecanimStateAction(minionObject, Animator.StringToHash("Base Layer.Exit"), logger));
					num = ((!(randomService.NextFloat(0f, 1f) < stageBuildingDefinition.posSkipPercent)) ? (num + 1) : (num + 2));
					if (num >= pathFinder.GetStageBuildingCapacity())
					{
						break;
					}
				}
			}
		}

		private void OnStuartConcertEnd()
		{
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				MinionObject minionObject = view.Get(item.ID);
				if (minionObject != null)
				{
					if (item.State != MinionState.Tasking && item.State != MinionState.Questing && item.State != MinionState.Leisure && item.State != MinionState.PlayingMignette)
					{
						minionObject.ClearActionQueue();
					}
				}
				else
				{
					logger.Warning("Minion {0} was null when we tried to stop concert animation.", item.ID);
				}
			}
		}

		private void EndTownhallMinionPartyAnimation()
		{
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				item.IsInMinionParty = false;
				item.IsDoingPartyFavorAnimation = false;
				MinionObject minionObject = view.Get(item.ID);
				if (minionObject != null)
				{
					minionObject.LeaveMinionParty();
				}
			}
		}

		private void ResetPartyAnimationCount()
		{
			view.ResetPartyAnimationCount();
		}

		private void PlayPartyAnimation(int minionID)
		{
			Minion byInstanceId = playerService.GetByInstanceId<Minion>(minionID);
			MinionPartyDefinition definition = playerService.GetMinionPartyInstance().Definition;
			int num = 0;
			int costumeId = byInstanceId.GetCostumeId(playerService, definitionService);
			CostumeItemDefinition definition2 = null;
			if (definitionService.TryGet<CostumeItemDefinition>(costumeId, out definition2))
			{
				num = definition2.PartyAnimations;
			}
			if (num < 1)
			{
				num = definition.PartyAnimations;
			}
			MinionAnimationDefinition minionAnimationDefinition = null;
			if (num > 0)
			{
				WeightedInstance weightedInstance = playerService.GetWeightedInstance(num);
				minionAnimationDefinition = definitionService.Get<MinionAnimationDefinition>(weightedInstance.NextPick(randomService).ID);
			}
			if (minionAnimationDefinition != null)
			{
				view.PlayPartyAnimation(byInstanceId.ID, minionAnimationDefinition, definition.MinionsPlayingAudioCount);
			}
		}

		private void IncidentalPartyFavorComplete(int id)
		{
			partyFavorService.ReleasePartyFavor(id);
		}

		public int GetIdleMinionCount()
		{
			int num = 0;
			foreach (Minion item in playerService.GetInstancesByType<Minion>())
			{
				if (item.State == MinionState.Idle || item.State == MinionState.Selectable || item.State == MinionState.Selected)
				{
					num++;
				}
			}
			return num;
		}
	}
}

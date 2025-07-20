using System;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using Kampai.Util.AI;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class MinionManagerView : ObjectManagerView, MinionIdleNotifier
	{
		public class Knuckleheaddedness
		{
			private HashSet<int> wrigglers;

			private float wriggle;

			public Knuckleheaddedness(GachaAnimationDefinition gacha, ICollection<int> minionIds, IRandomService randomService)
			{
				if (gacha.knuckleheadednessInfo == null)
				{
					AssignDefaults(gacha);
				}
				wriggle = gacha.knuckleheadednessInfo.KnuckleheaddednessScale;
				float num = gacha.knuckleheadednessInfo.KnuckleheaddednessMin;
				if (!(num > 0f) || !(wriggle > 0f))
				{
					return;
				}
				int count = minionIds.Count;
				if (count > 1)
				{
					if (gacha.knuckleheadednessInfo.KnuckleheaddednessMax > num)
					{
						num += randomService.NextFloat(num, gacha.knuckleheadednessInfo.KnuckleheaddednessMax);
					}
					count = Convert.ToInt32(Math.Floor((float)count * num));
					if (count > 0)
					{
						wrigglers = new HashSet<int>();
						ListUtil.RandomSublist(randomService, minionIds, wrigglers, count);
					}
				}
			}

			public float DelayTime(int minionId, IRandomService rand)
			{
				if (wrigglers != null && wrigglers.Contains(minionId))
				{
					return rand.NextFloat(wriggle);
				}
				return 0f;
			}

			private void AssignDefaults(GachaAnimationDefinition gacha)
			{
				KnuckleheadednessInfo knuckleheadednessInfo = new KnuckleheadednessInfo();
				if (gacha.Minions != 0)
				{
					knuckleheadednessInfo.KnuckleheaddednessMin = 0f;
					knuckleheadednessInfo.KnuckleheaddednessMax = 0f;
					knuckleheadednessInfo.KnuckleheaddednessScale = 0f;
				}
				else
				{
					knuckleheadednessInfo.KnuckleheaddednessMin = 0.2f;
					knuckleheadednessInfo.KnuckleheaddednessMax = 1f;
					knuckleheadednessInfo.KnuckleheaddednessScale = 0.2f;
				}
				gacha.knuckleheadednessInfo = knuckleheadednessInfo;
			}
		}

		internal Signal<MinionObject> idleMinionSignal = new Signal<MinionObject>();

		private IList<int> coordinatedAnimations = new List<int>();

		private static bool alternatePlayed;

		private Boxed<Vector3> partyLocation;

		private float partyRadius;

		private IList<int> minionsPlayingAudio;

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject]
		public IPlayerService player { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public MinionStateChangeSignal stateChangeSignal { get; set; }

		[Inject]
		public PlayLocalAudioSignal playLocalAudio { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		protected override void CacheAnimation(AnimationDefinition animDef)
		{
			base.CacheAnimation(animDef);
			GachaAnimationDefinition gachaAnimationDefinition = animDef as GachaAnimationDefinition;
			if (gachaAnimationDefinition != null && gachaAnimationDefinition.Prefab != null && !coordinatedAnimations.Contains(gachaAnimationDefinition.ID))
			{
				UnityEngine.Object @object = KampaiResources.Load(gachaAnimationDefinition.Prefab);
				if (@object == null)
				{
					logger.Fatal(FatalCode.AN_UNABLE_TO_LOAD_PREFAB, gachaAnimationDefinition.Prefab);
				}
				GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
				if (gameObject == null)
				{
					logger.Fatal(FatalCode.AN_UNABLE_TO_LOAD_PREFAB, gachaAnimationDefinition.Prefab);
				}
				UnityEngine.Object.Destroy(gameObject);
				coordinatedAnimations.Add(gachaAnimationDefinition.ID);
			}
		}

		protected override string GetAnimationStateMachine(AnimationDefinition animDef)
		{
			string animationStateMachine = base.GetAnimationStateMachine(animDef);
			if (animationStateMachine.Length == 0)
			{
				GachaAnimationDefinition gachaAnimationDefinition = animDef as GachaAnimationDefinition;
				if (gachaAnimationDefinition != null)
				{
					return definitionService.Get<MinionAnimationDefinition>(gachaAnimationDefinition.AnimationID).StateMachine;
				}
				return string.Empty;
			}
			return animationStateMachine;
		}

		public override void Add(ActionableObject obj)
		{
			MinionObject minionObject = obj as MinionObject;
			if (minionObject == null)
			{
				logger.Error("Tried to add an ActionableObject that wasn't a MinionObject to MinionManagerView");
				return;
			}
			minionObject.SetDefaultAnimController(animationControllers["asm_minion_movement"]);
			minionObject.SetAnimController(animationControllers["asm_minion_movement"]);
			base.Add(obj);
			minionObject.EnableRenderers(false);
		}

		public new MinionObject Get(int objectId)
		{
			ActionableObject value;
			objects.TryGetValue(objectId, out value);
			return value as MinionObject;
		}

		internal void StopMinionCamping()
		{
			foreach (KeyValuePair<int, ActionableObject> @object in objects)
			{
				ActionableObject value = @object.Value;
				if (value is MinionObject)
				{
					value.SetAnimTrigger("OnNewMinionExit");
				}
			}
		}

		internal void playMinionAudio(int minionID, string audioEvent)
		{
			ActionableObject actionableObject = objects[minionID];
			playLocalAudio.Dispatch(actionableObject.localAudioEmitter, audioEvent, new Dictionary<string, float>());
		}

		internal void StartMinionTask(MinionObject mo, TaskableBuilding building, Signal<MinionObject, Building> startSignal, Signal<int> stopSignal, Signal<CharacterObject> relocateSignal, IList<Vector3> path, float rotation)
		{
			float speed = 4.5f;
			mo.EnqueueAction(new SetAnimatorAction(mo, animationControllers["asm_minion_movement"], logger), true);
			mo.EnqueueAction(new ConstantSpeedPathAction(mo, path, speed, logger));
			mo.EnqueueAction(new RotateAction(mo, rotation, 720f, logger));
			AddRemoveFromBuildingActions(mo, building, mo.GetMinion(), startSignal, stopSignal, relocateSignal);
		}

		private void AddRemoveFromBuildingActions(MinionObject mo, TaskableBuilding building, Minion minion, Signal<MinionObject, Building> startSignal, Signal<int> stopSignal, Signal<CharacterObject> relocateSignal)
		{
			RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(building.Definition.AnimationDefinitions[0].MinionController);
			mo.EnqueueAction(new SetAnimatorAction(mo, controller, logger));
			if (building.GetMinionsInBuilding() > building.Definition.WorkStations)
			{
				mo.EnqueueAction(new EnableRendererAction(mo, false, logger));
			}
			mo.EnqueueAction(new MinionTaskAction(mo, building, startSignal, logger));
			mo.EnqueueAction(new SignalAction(mo, stopSignal, logger));
			mo.EnqueueAction(new EnableRendererAction(mo, true, logger));
			mo.EnqueueAction(new StateChangeAction(minion.ID, stateChangeSignal, MinionState.Idle, logger));
			if (building is DebrisBuilding)
			{
				mo.EnqueueAction(new PelvisAnimationCompleteAction(logger, mo, animationControllers["asm_minion_movement"]));
				return;
			}
			mo.EnqueueAction(new SetAnimatorAction(mo, animationControllers["asm_minion_movement"], logger));
			mo.EnqueueAction(new GotoSideWalkAction(mo, building, logger, definitionService, relocateSignal));
		}

		internal void TeleportMinionTask(Minion minion, TaskableBuilding building, Signal<MinionObject, Building> startSignal, Signal<int> stopSignal, Signal<CharacterObject> relocateSignal)
		{
			ActionableObject actionableObject = objects[minion.ID];
			MinionObject minionObject = actionableObject as MinionObject;
			if (minionObject == null)
			{
				logger.Error("TeleportMinionTask: ao as MinionObject == null");
			}
			else
			{
				AddRemoveFromBuildingActions(minionObject, building, minion, startSignal, stopSignal, relocateSignal);
			}
		}

		internal void UpdateTaskedMinion(int minionId, MinionTaskInfo taskInfo)
		{
			ActionableObject actionableObject = objects[minionId];
			actionableObject.EnableRenderers(true);
			actionableObject.transform.position = taskInfo.Position;
			actionableObject.transform.rotation = taskInfo.Rotation;
			actionableObject.SetAnimInteger("minionPosition", taskInfo.PositionIndex);
		}

		internal void MinionAppear(int minionID, Vector3 pos)
		{
			if (objects.ContainsKey(minionID))
			{
				objects[minionID].EnqueueAction(new AppearAction(objects[minionID], pos, logger), true);
			}
		}

		public MinionObject GetMinionObject(Minion minion)
		{
			return (!objects.ContainsKey(minion.ID)) ? null : (objects[minion.ID] as MinionObject);
		}

		public MinionObject GetMinionObjectByID(int minionID)
		{
			if (objects.ContainsKey(minionID))
			{
				return objects[minionID] as MinionObject;
			}
			return null;
		}

		internal void SelectMinion(int minionID, MinionAnimationDefinition minionAnimDef, Boxed<Vector3> runLocation, MinionMoveToSignal minionMoveToSignal, bool muteStatus)
		{
			ActionableObject value;
			if (!objects.TryGetValue(minionID, out value))
			{
				return;
			}
			MinionObject minionObject = value as MinionObject;
			if (minionObject == null)
			{
				logger.Error("SelectMinion: ao as MinionObject == null");
				return;
			}
			if (runLocation != null && (minionObject.transform.position - runLocation.Value).sqrMagnitude < 0.0001f)
			{
				runLocation = null;
			}
			EnqueueAnimation(minionObject, minionAnimDef, muteStatus);
			minionObject.EnqueueAction(new SelectedAction(minionID, runLocation, minionMoveToSignal, logger));
			if (muteStatus)
			{
				minionObject.EnqueueAction(new MuteAction(minionObject, muteStatus, logger));
			}
		}

		public void AnimateMinion(int minionID, MinionAnimationDefinition gachaPick, bool muteStatus)
		{
			ActionableObject value;
			if (objects.TryGetValue(minionID, out value))
			{
				MinionObject minionObject = (MinionObject)value;
				value.EnqueueAction(new SetMinionGachaState(minionObject, MinionObject.MinionGachaState.IndividualTap, logger));
				EnqueueAnimation(value, gachaPick, muteStatus);
				value.EnqueueAction(new SetMinionGachaState(minionObject, MinionObject.MinionGachaState.Inactive, logger));
			}
		}

		private void EnqueueAnimation(ActionableObject mo, MinionAnimationDefinition gachaPick, bool muteStatus)
		{
			if (muteStatus)
			{
				mo.EnqueueAction(new MuteAction(mo, muteStatus, logger), true);
			}
			if (gachaPick != null && !mo.IsInAnimatorState(Animator.StringToHash("Base Layer.Init")) && !mo.IsInAnimatorState(Animator.StringToHash("Base Layer.gacha")))
			{
				mo.EnqueueAction(new SetAnimatorAction(mo, animationControllers[gachaPick.StateMachine], logger, gachaPick.arguments), !muteStatus);
			}
			mo.EnqueueAction(new RotateAction(mo, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			if (gachaPick != null)
			{
				mo.EnqueueAction(new WaitForMecanimStateAction(mo, Animator.StringToHash("Base Layer.Exit"), logger));
			}
		}

		internal void SetMinionMute(int minionID, bool mute)
		{
			(objects[minionID] as MinionObject).SetMuteStatus(mute);
		}

		internal void StartGroupGacha(GachaAnimationDefinition gachaPick, ICollection<int> minionIds, Vector3 centerPoint, IPathFinder pathFinder)
		{
			ICollection<ActionableObject> collection = ToActionableObjects(minionIds);
			SyncAction action = new SyncAction(collection, logger);
			foreach (MinionObject item in collection)
			{
				item.EnqueueAction(new StopWalkingAction(item, logger));
				item.EnqueueAction(action);
				item.EnqueueAction(new SetMinionGachaState(item, MinionObject.MinionGachaState.Active, logger));
			}
			if (coordinatedAnimations.Contains(gachaPick.ID))
			{
				SetupCoordinatedMinionGacha(base.gameObject, definitionService, gachaPick, animationControllers, collection, centerPoint, pathFinder, logger);
			}
			else
			{
				StartNonCoordinatedGacha(gachaPick, collection, minionIds);
			}
			foreach (MinionObject item2 in collection)
			{
				item2.EnqueueAction(new SetMinionGachaState(item2, MinionObject.MinionGachaState.Inactive, logger));
				item2.EnqueueAction(new IncidentalFinishedAction(item2.ID, stateChangeSignal, logger));
			}
		}

		internal void StartNonCoordinatedGacha(GachaAnimationDefinition gachaPick, ICollection<ActionableObject> actionableObjects, ICollection<int> minionIds)
		{
			StartNonCoordinatedGacha(gachaPick, actionableObjects, minionIds, null);
		}

		internal void StartNonCoordinatedGacha(GachaAnimationDefinition gachaPick, ICollection<ActionableObject> actionableObjects, ICollection<int> minionIds, Boxed<Vector3> buildingPos)
		{
			if (gachaPick == null)
			{
				logger.Log(KampaiLogLevel.Error, "Null gacha pick (CONFIG ERROR)");
				return;
			}
			alternatePlayed = false;
			Knuckleheaddedness knuckleheaddedness = new Knuckleheaddedness(gachaPick, minionIds, randomService);
			bool mute = false;
			foreach (ActionableObject actionableObject in actionableObjects)
			{
				MinionObject minionObject = actionableObject as MinionObject;
				if (!(minionObject == null))
				{
					float animationDelay = knuckleheaddedness.DelayTime(minionObject.ID, randomService);
					SetupSingleMinionGacha(randomService, definitionService, minionObject, animationControllers, gachaPick, animationDelay, buildingPos, logger, ref mute);
				}
			}
		}

		internal void SetMinionReady(int minionId)
		{
			MinionObject minionObject = objects[minionId] as MinionObject;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("isReady", true);
			QuantityItem quantityItem = player.GetWeightedInstance(4002).NextPick(randomService);
			minionObject.SetAnimInteger("IdleRandom", quantityItem.ID);
			minionObject.EnqueueAction(new SetAnimatorAction(minionObject, animationControllers["asm_minion_movement"], logger, dictionary));
			minionObject.EnqueueAction(new DelayAction(minionObject, 1.4f, logger));
		}

		private static int GetClosestRoutingPoint(Vector3 subject, Transform[] routingPoints, IList<Transform> usedRoutingPoints)
		{
			float num = float.MaxValue;
			int result = -1;
			for (int i = 0; i < routingPoints.Length; i++)
			{
				Transform transform = routingPoints[i];
				if (!usedRoutingPoints.Contains(transform))
				{
					float num2 = Vector3.Distance(subject, transform.position);
					if (num2 < num)
					{
						num = num2;
						result = i;
					}
				}
			}
			return result;
		}

		private static bool RoutesAreValid(IPathFinder pathFinder, Transform[] routes)
		{
			foreach (Transform transform in routes)
			{
				if (!pathFinder.IsOccupiable(new Location(transform.position)))
				{
					return false;
				}
			}
			return true;
		}

		public static void SetupCoordinatedMinionGacha(GameObject parent, IDefinitionService definitionService, GachaAnimationDefinition def, Dictionary<string, RuntimeAnimatorController> animationControllers, ICollection<ActionableObject> actionableObjects, Vector3 centerPoint, IPathFinder pathFinder, IKampaiLogger logger)
		{
			CoordinatedAnimation coordinatedAnimation = parent.AddComponent<CoordinatedAnimation>();
			Vector3 position = Camera.main.transform.position;
			coordinatedAnimation.Init(def, parent.transform, centerPoint, new Vector3(position.x, 0f, position.z), logger);
			Transform[] routingSlots = coordinatedAnimation.GetRoutingSlots();
			if (!RoutesAreValid(pathFinder, routingSlots))
			{
				return;
			}
			int num = routingSlots.Length;
			if (num != actionableObjects.Count)
			{
				logger.Log(KampaiLogLevel.Error, "Too many minions for selected gacha");
				List<ActionableObject> list = new List<ActionableObject>();
				int num2 = 0;
				foreach (ActionableObject actionableObject in actionableObjects)
				{
					if (num2++ == num)
					{
						break;
					}
					list.Add(actionableObject);
				}
				actionableObjects = list;
			}
			IList<Transform> list2 = new List<Transform>();
			VFXScript vFXScript = coordinatedAnimation.GetVFXScript();
			DestroyObjectAction deallocateAnimationPrefab = new DestroyObjectAction(coordinatedAnimation, logger);
			SyncAction syncActionA = new SyncAction(actionableObjects, logger);
			SyncAction syncActionB = new SyncAction(actionableObjects, logger);
			Signal cancelSignal = new ActionSignal(CancelGachaAction(actionableObjects), true);
			foreach (ActionableObject actionableObject2 in actionableObjects)
			{
				MinionObject minionObject = actionableObject2 as MinionObject;
				if (minionObject == null)
				{
					continue;
				}
				Vector3 position2 = minionObject.transform.position;
				int closestRoutingPoint = GetClosestRoutingPoint(position2, routingSlots, list2);
				list2.Add(routingSlots[closestRoutingPoint]);
				RuntimeAnimatorController controller = animationControllers[definitionService.Get<MinionAnimationDefinition>(def.AnimationID).StateMachine];
				IList<Vector3> path = pathFinder.FindPath(minionObject.transform.position, routingSlots[closestRoutingPoint].position, 4, true);
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				MinionAnimationDefinition minionAnimationDefinition = definitionService.Get<MinionAnimationDefinition>(def.AnimationID);
				if (minionAnimationDefinition.arguments != null)
				{
					foreach (string key in minionAnimationDefinition.arguments.Keys)
					{
						if (!key.Equals("actor"))
						{
							dictionary.Add(key, minionAnimationDefinition.arguments[key]);
							continue;
						}
						logger.Warning("Ignoring actor attribute for {0}", minionAnimationDefinition.ID);
					}
				}
				dictionary.Add("actor", closestRoutingPoint);
				minionObject.StopLocalAudio();
				EnqueueActions(exitState: def.SoloExit ? string.Format("Base Layer.Exit_{0}", closestRoutingPoint) : "Base Layer.Exit", mo: minionObject, animationControllers: animationControllers, vfxScript: vFXScript, path: path, rotation: routingSlots[closestRoutingPoint].rotation.eulerAngles.y, controller: controller, args: dictionary, deallocateAnimationPrefab: deallocateAnimationPrefab, logger: logger, syncActionA: syncActionA, syncActionB: syncActionB, cancelSignal: cancelSignal);
			}
		}

		private static Action CancelGachaAction(ICollection<ActionableObject> objs)
		{
			return delegate
			{
				foreach (ActionableObject obj in objs)
				{
					obj.ClearActionQueue();
				}
			};
		}

		private static void EnqueueActions(MinionObject mo, Dictionary<string, RuntimeAnimatorController> animationControllers, VFXScript vfxScript, IList<Vector3> path, float rotation, RuntimeAnimatorController controller, Dictionary<string, object> args, DestroyObjectAction deallocateAnimationPrefab, IKampaiLogger logger, SyncAction syncActionA, SyncAction syncActionB, string exitState, Signal cancelSignal)
		{
			mo.EnqueueAction(new SetAnimatorAction(mo, animationControllers["asm_minion_movement"], logger));
			if (vfxScript != null)
			{
				mo.EnqueueAction(new TrackVFXAction(mo, vfxScript, logger));
			}
			mo.EnqueueAction(new CancelablePathAction(cancelSignal, 0.2f, mo, path, 0.5f, logger));
			mo.EnqueueAction(new RotateAction(mo, rotation, 720f, logger));
			mo.EnqueueAction(syncActionA);
			mo.EnqueueAction(new SetAnimatorAction(mo, controller, logger, args));
			mo.EnqueueAction(new WaitForMecanimStateAction(mo, Animator.StringToHash(exitState), logger));
			if (vfxScript != null)
			{
				mo.EnqueueAction(new UntrackVFXAction(mo, logger));
			}
			mo.EnqueueAction(deallocateAnimationPrefab);
			mo.EnqueueAction(new PelvisAnimationCompleteAction(logger, mo, animationControllers["asm_minion_movement"]));
			mo.EnqueueAction(syncActionB);
		}

		public static void SetupSingleMinionGacha(IRandomService randomService, IDefinitionService definitionService, MinionObject mo, Dictionary<string, RuntimeAnimatorController> animationControllers, GachaAnimationDefinition gachaPick, float animationDelay, Boxed<Vector3> buildingPos, IKampaiLogger logger, ref bool mute)
		{
			mo.StopLocalAudio();
			if (animationDelay > 0f)
			{
				mo.EnqueueAction(new DelayAction(mo, animationDelay, logger));
			}
			bool flag = false;
			AnimationAlternate animationAlternate = gachaPick.AnimationAlternate;
			if (animationAlternate != null && randomService.NextFloat(0f, 1f) < animationAlternate.PercentChance && !alternatePlayed)
			{
				alternatePlayed = true;
				DefinitionGroup definitionGroup = definitionService.Get<DefinitionGroup>(animationAlternate.GroupID);
				int index = randomService.NextInt(0, definitionGroup.Group.Count);
				gachaPick = definitionService.Get<GachaAnimationDefinition>(definitionGroup.Group[index]);
				mo.EnqueueAction(new MuteAction(mo, false, logger));
				mo.EnqueueAction(new SetMinionGachaState(mo, MinionObject.MinionGachaState.Deviant, logger));
				flag = true;
			}
			if (!flag)
			{
				mo.EnqueueAction(new MuteAction(mo, mute, logger));
				mute = true;
			}
			MinionAnimationDefinition minionAnimationDefinition = definitionService.Get<MinionAnimationDefinition>(gachaPick.AnimationID);
			string stateMachine = minionAnimationDefinition.StateMachine;
			if (!animationControllers.ContainsKey(stateMachine))
			{
				logger.Log(KampaiLogLevel.Error, "No state machine {0}", stateMachine);
				return;
			}
			mo.EnqueueAction(new SetAnimatorAction(mo, animationControllers[stateMachine], logger, minionAnimationDefinition.arguments));
			if (minionAnimationDefinition.FaceCamera)
			{
				mo.EnqueueAction(new RotateAction(mo, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			}
			else if (buildingPos != null)
			{
				Vector3 forward = buildingPos.Value - mo.transform.position;
				float y = Quaternion.LookRotation(forward).eulerAngles.y;
				mo.EnqueueAction(new RotateAction(mo, y, 360f, logger));
			}
			mo.EnqueueAction(new WaitForMecanimStateAction(mo, Animator.StringToHash("Base Layer.Exit"), logger));
			mo.EnqueueAction(new MuteAction(mo, false, logger));
		}

		internal Queue<int> GetMinionListSortedByDistanceAndState(Vector3 position, bool needSelected = true)
		{
			Vector3 vector = cameraUtils.GroundPlaneRaycast(position);
			Queue<int> queue = new Queue<int>();
			List<Tuple<Minion, float>> list = new List<Tuple<Minion, float>>();
			foreach (KeyValuePair<int, ActionableObject> @object in objects)
			{
				Minion byInstanceId = player.GetByInstanceId<Minion>(@object.Key);
				if (byInstanceId.State == MinionState.Idle || byInstanceId.State == MinionState.Selectable || byInstanceId.State == MinionState.Uninitialized || (byInstanceId.State == MinionState.Selected && needSelected))
				{
					list.Add(Tuple.Create(byInstanceId, (vector - @object.Value.transform.position).sqrMagnitude));
				}
			}
			list.Sort((Tuple<Minion, float> minion1, Tuple<Minion, float> minion2) => (minion1.Item1.Level != minion2.Item1.Level) ? minion1.Item1.Level.CompareTo(minion2.Item1.Level) : minion1.Item2.CompareTo(minion2.Item2));
			if (needSelected)
			{
				EnqueueMinionsInCertainStates(list, queue, MinionState.Selected);
			}
			EnqueueMinionsInCertainStates(list, queue, MinionState.Idle, MinionState.Selectable, MinionState.Uninitialized);
			return queue;
		}

		private void EnqueueMinionsInCertainStates(List<Tuple<Minion, float>> minionTuples, Queue<int> minionQueue, params MinionState[] states)
		{
			for (int i = 0; i < minionTuples.Count; i++)
			{
				if (ListUtil.Contains(states, minionTuples[i].Item1.State))
				{
					minionQueue.Enqueue(minionTuples[i].Item1.ID);
				}
			}
		}

		internal void StartMinionAnimation(int minionId, MinionAnimationDefinition def, bool isIncidental)
		{
			ActionableObject actionableObject = objects[minionId];
			actionableObject.EnqueueAction(new SetAnimatorAction(actionableObject, animationControllers[def.StateMachine], logger, def.arguments), isIncidental);
			if (def.FaceCamera)
			{
				actionableObject.EnqueueAction(new RotateAction(actionableObject, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			}
			actionableObject.EnqueueAction(new DelayAction(actionableObject, def.AnimationSeconds, logger));
		}

		internal void MinionAcknowledgement(int minionID, float rotateTo, MinionAnimationDefinition def)
		{
			ActionableObject actionableObject = objects[minionID];
			actionableObject.EnqueueAction(new SetAnimatorAction(actionableObject, animationControllers[def.StateMachine], logger, def.arguments), true);
			actionableObject.EnqueueAction(new RotateAction(actionableObject, rotateTo, 360f, logger));
			actionableObject.EnqueueAction(new DelayAction(actionableObject, def.AnimationSeconds, logger));
			actionableObject.EnqueueAction(new IncidentalFinishedAction(minionID, stateChangeSignal, logger));
		}

		internal Minion GetClosestMinionToLocation(Location location, bool highest)
		{
			float num = float.MaxValue;
			int num2 = -1;
			int num3 = int.MaxValue;
			Minion result = null;
			Vector3 vector = new Vector3(location.x, 0f, location.y);
			foreach (KeyValuePair<int, ActionableObject> @object in objects)
			{
				Minion byInstanceId = player.GetByInstanceId<Minion>(@object.Key);
				int level = byInstanceId.Level;
				if (byInstanceId.State != MinionState.Idle && byInstanceId.State != MinionState.Selected)
				{
					continue;
				}
				if (highest)
				{
					if (level < num2)
					{
						continue;
					}
				}
				else if (level > num3)
				{
					continue;
				}
				float sqrMagnitude = (vector - @object.Value.transform.position).sqrMagnitude;
				if (highest)
				{
					if (level > num2)
					{
						num = sqrMagnitude;
						num2 = level;
						result = byInstanceId;
					}
					else if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = byInstanceId;
					}
				}
				else if (level < num3)
				{
					num = sqrMagnitude;
					num3 = level;
					result = byInstanceId;
				}
				else if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = byInstanceId;
				}
			}
			return result;
		}

		internal void MinionReact(GachaAnimationDefinition gachaPick, ICollection<int> minionIds, Boxed<Vector3> buildingPos)
		{
			StartNonCoordinatedGacha(gachaPick, ToActionableObjects(minionIds), minionIds, buildingPos);
		}

		public void MinionIdle(MinionObject minionObject)
		{
			idleMinionSignal.Dispatch(minionObject);
		}

		internal void SeekPosition(int minionID, Vector3 pos, float threshold)
		{
			ActionableObject value;
			if (objects.TryGetValue(minionID, out value))
			{
				SteerCharacterToSeek component = value.GetComponent<SteerCharacterToSeek>();
				component.Target = pos;
				component.Threshold = threshold;
				component.enabled = true;
			}
		}

		internal void SetPartyState(int minionID, bool isInParty, bool gameIsStarting, MinionAnimationDefinition partyStartAnimation)
		{
			if (isInParty && partyLocation == null)
			{
				logger.Warning("Party location is not known.");
			}
			else
			{
				ActionableObject value;
				if (!objects.TryGetValue(minionID, out value))
				{
					return;
				}
				MinionObject minionObject = value as MinionObject;
				if (isInParty)
				{
					minionObject.EnterParty(partyLocation.Value, partyRadius);
					if (gameIsStarting && partyStartAnimation != null)
					{
						minionObject.EnqueueAction(new DelayAction(minionObject, UnityEngine.Random.value * 0.3f, logger));
						if (partyStartAnimation.FaceCamera)
						{
							minionObject.EnqueueAction(new RotateAction(minionObject, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
						}
						minionObject.EnqueueAction(new SetAnimatorAction(minionObject, animationControllers[partyStartAnimation.StateMachine], logger, partyStartAnimation.arguments));
						minionObject.EnqueueAction(new WaitForMecanimStateAction(minionObject, Animator.StringToHash("Base Layer.Exit"), logger));
					}
				}
				else
				{
					minionObject.LeaveParty();
				}
			}
		}

		internal void SetPartyLocation(Boxed<Vector3> partyLocation, float partyRadius)
		{
			this.partyLocation = partyLocation;
			this.partyRadius = partyRadius;
		}

		internal void PlayPartyAnimation(int minionID, MinionAnimationDefinition def, int allowedAudioMinions)
		{
			ActionableObject value;
			if (objects.TryGetValue(minionID, out value))
			{
				MinionObject mo = value as MinionObject;
				if (minionsPlayingAudio == null)
				{
					minionsPlayingAudio = new List<int>();
				}
				bool flag = minionsPlayingAudio.Contains(minionID);
				if (!flag && minionsPlayingAudio.Count < allowedAudioMinions)
				{
					flag = true;
					minionsPlayingAudio.Add(minionID);
				}
				EnqueueAnimation(mo, def, !flag);
			}
		}

		internal void ResetPartyAnimationCount()
		{
			minionsPlayingAudio = null;
		}
	}
}

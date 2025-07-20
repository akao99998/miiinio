using System;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class PartyFavorAnimationView : KampaiView
	{
		private sealed class MinionArgs
		{
			public GameObject parent;

			public IDefinitionService definitionService;

			public PartyFavorAnimationDefinition def;

			public RuntimeAnimatorController walkAnimController;

			public RuntimeAnimatorController animController;

			public Vector3 centerPoint;

			public IPathFinder pathFinder;

			public MinionPartyDefinition partyDefinition;
		}

		private IKampaiLogger logger;

		private IBuildingUtilities buildingUtilies;

		private RuntimeAnimatorController minionWalkStateMachine;

		private IDefinitionService definitionService;

		private MinionStateChangeSignal stateChangeSignal;

		private PathFinder pathFinder;

		private MinionPartyDefinition partyDefinition;

		private Environment environment;

		private DebugUpdateGridSignal debugUpdateGridSignal;

		private Action<int> onFinishAnimCallback;

		protected MinionObject minionObj;

		private string footprint;

		private int[,] tempGrid;

		public PartyFavorAnimationDefinition PartyFavorDefinition { get; set; }

		public Location GetLocation()
		{
			Vector3 position = base.transform.position;
			return new Location(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
		}

		public void Init(PartyFavorAnimationDefinition definition, IDefinitionService definitionService, PathFinder pathFinder, DebugUpdateGridSignal debugUpdateGridSignal, Environment environment, Action<int> onFinishAnimCallback)
		{
			this.onFinishAnimCallback = onFinishAnimCallback;
			this.pathFinder = pathFinder;
			this.definitionService = definitionService;
			this.debugUpdateGridSignal = debugUpdateGridSignal;
			this.environment = environment;
			footprint = definitionService.GetBuildingFootprint(definition.FootprintID);
			tempGrid = new int[BuildingUtil.GetFootprintWidth(footprint), BuildingUtil.GetFootprintDepth(footprint)];
			partyDefinition = definitionService.Get<MinionPartyDefinition>(80000);
			PartyFavorDefinition = definition;
			minionWalkStateMachine = KampaiResources.Load<RuntimeAnimatorController>("asm_minion_movement");
		}

		internal void SetupInjections(IKampaiLogger logger, MinionStateChangeSignal minionStateChangeSignal, IBuildingUtilities buildingUtilies)
		{
			stateChangeSignal = minionStateChangeSignal;
			this.logger = logger;
			this.buildingUtilies = buildingUtilies;
		}

		public void TrackChild(MinionObject minionObj)
		{
			this.minionObj = minionObj;
			base.transform.position = this.minionObj.transform.position;
			UpdatePath(GetLocation(), true);
			MinionArgs minionArgs = new MinionArgs();
			minionArgs.parent = base.gameObject;
			minionArgs.definitionService = definitionService;
			minionArgs.def = PartyFavorDefinition;
			minionArgs.walkAnimController = minionWalkStateMachine;
			minionArgs.animController = KampaiResources.Load<RuntimeAnimatorController>(definitionService.Get<MinionAnimationDefinition>(PartyFavorDefinition.AnimationID).StateMachine);
			minionArgs.centerPoint = base.transform.position;
			minionArgs.pathFinder = pathFinder;
			minionArgs.partyDefinition = partyDefinition;
			MinionArgs args = minionArgs;
			SetupMinionPartyFavorAnimation(args);
		}

		public void UntrackChild()
		{
			if (minionObj != null)
			{
				MinionState newState = MinionState.Idle;
				minionObj.ApplyRootMotion(false);
				minionObj.EnableBlobShadow(true);
				minionObj.SetAnimatorCullingMode(AnimatorCullingMode.CullUpdateTransforms);
				if (minionObj.GetCurrentAnimControllerName().Contains("Prop"))
				{
					float time = 1f;
					int layer = -1;
					minionObj.PlayAnimation(Animator.StringToHash("Base Layer.Exit"), layer, time);
				}
				minionObj.EnqueueAction(new StateChangeAction(minionObj.ID, stateChangeSignal, newState, logger));
				minionObj.EnqueueAction(new SetAnimatorAction(minionObj, minionWalkStateMachine, logger));
			}
		}

		public void FreeAllMinions()
		{
			UpdatePath(GetLocation(), false);
			UntrackChild();
		}

		private void SetupMinionPartyFavorAnimation(MinionArgs args)
		{
			CoordinatedAnimation coordinatedAnimation = args.parent.AddComponent<CoordinatedAnimation>();
			Vector3 position = Camera.main.transform.position;
			coordinatedAnimation.Init(args.def, args.parent.transform, args.centerPoint, new Vector3(position.x, 0f, position.z), logger);
			VFXScript vFXScript = coordinatedAnimation.GetVFXScript();
			DestroyObjectAction deallocateAnimationPrefab = new DestroyObjectAction(coordinatedAnimation, logger);
			minionObj.StopLocalAudio();
			EnqueueActions(args, minionObj, vFXScript, GetAnimationArgs(args.def.AnimationID), deallocateAnimationPrefab, "Base Layer.Exit");
		}

		private void EnqueueActions(MinionArgs args, MinionObject mo, VFXScript vfxScript, Dictionary<string, object> animArgs, DestroyObjectAction deallocateAnimationPrefab, string exitState)
		{
			mo.EnqueueAction(new MuteAction(mo, false, logger));
			mo.EnqueueAction(new RotateAction(mo, Camera.main.transform.eulerAngles.y - 180f, 360f, logger));
			mo.EnqueueAction(new SetAnimatorAction(mo, args.animController, logger, animArgs));
			mo.EnqueueAction(new WaitForMecanimStateAction(mo, Animator.StringToHash(exitState), logger));
			if (vfxScript != null)
			{
				mo.EnqueueAction(new UntrackVFXAction(mo, logger));
			}
			mo.EnqueueAction(deallocateAnimationPrefab);
			mo.EnqueueAction(new PelvisAnimationCompleteAction(logger, mo, args.walkAnimController));
			mo.EnqueueAction(new DelegateAction(delegate
			{
				GetNextController(mo.ID);
			}, logger));
		}

		private void GetNextController(int minionId)
		{
			if (onFinishAnimCallback != null)
			{
				onFinishAnimCallback(minionId);
			}
		}

		private Dictionary<string, object> GetAnimationArgs(int animationId)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			MinionAnimationDefinition minionAnimationDefinition = definitionService.Get<MinionAnimationDefinition>(animationId);
			if (minionAnimationDefinition.arguments != null)
			{
				foreach (KeyValuePair<string, object> argument in minionAnimationDefinition.arguments)
				{
					if (argument.Key.Equals("actor"))
					{
						logger.Warning("Ignoring actor attribute for {0}", minionAnimationDefinition.ID);
					}
					else
					{
						dictionary.Add(argument.Key, argument.Value);
					}
				}
			}
			dictionary.Add("actor", 0);
			return dictionary;
		}

		private void UpdatePath(Location location, bool isAddingToFootprint)
		{
			int x = location.x;
			int num = x;
			int num2 = location.y;
			int num3 = 0;
			int num4 = 0;
			string text = footprint;
			foreach (char c in text)
			{
				char c2 = c;
				if (c2 != 'X' && c2 != 'x' && c2 == '|')
				{
					num = x;
					num3 = 0;
					num2--;
					num4++;
				}
				else if (buildingUtilies.CheckGridBounds(num, num2))
				{
					if (isAddingToFootprint)
					{
						tempGrid[num3, num4] = environment.PlayerGrid[num, num2].Modifier;
						environment.PlayerGrid[num, num2].Walkable = false;
						environment.PlayerGrid[num, num2].Occupied = true;
					}
					else
					{
						environment.PlayerGrid[num, num2].Modifier = tempGrid[num3, num4];
					}
					num++;
					num3++;
				}
			}
			pathFinder.UpdateWalkableRegion();
			debugUpdateGridSignal.Dispatch();
		}
	}
}

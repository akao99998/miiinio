using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common.Service.Audio;
using Kampai.Game.Mignette.View;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.Mignette.EdwardMinionHands.View
{
	public class EdwardMinionHandsMignetteManagerView : MignetteManagerView
	{
		private const string CHAT_LEFT1_ANIM_STATE_NAME = "Base Layer.ChatLeft01";

		private const string NOD_RIGHT1_ANIM_STATE_NAME = "Base Layer.NodRight01";

		private const string NOD_LEFT2_ANIM_STATE_NAME = "Base Layer.NodLeft02";

		private IKampaiLogger logger = LogManager.GetClassLogger("EdwardMinionHandsMignetteManagerView") as IKampaiLogger;

		private List<EdwardMinionHandsCuttingToolViewObject> ToolsWithMinionsList = new List<EdwardMinionHandsCuttingToolViewObject>();

		private Dictionary<EdwardMinionHandsCollectableViewObject, EdwardMinionHandsCuttingToolViewObject> CollectableDictionary = new Dictionary<EdwardMinionHandsCollectableViewObject, EdwardMinionHandsCuttingToolViewObject>();

		private List<int> CollectablePointPool = new List<int>();

		private EdwardMinionHandsBuildingViewObject BuildingViewReference;

		private EdwardMinionHandsGameController gameController;

		private float TimeTillCollectableEmit;

		private bool TrimmingInProgress;

		private float currentProgress;

		private List<EdwardMinionHandsCuttingToolViewObject> ToolsWaitingForStateChangeQueue = new List<EdwardMinionHandsCuttingToolViewObject>();

		public int prevStateNameHash;

		private CustomFMOD_StudioEventEmitter trimEmitter;

		private bool trimSoundPlaying;

		private bool hidePolesIfInIdle;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public ChangeMignetteScoreSignal changeScoreSignal { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		[Inject]
		public SpawnMignetteDooberSignal spawnMignetteDooberSignal { get; set; }

		[Inject]
		public RequestStopMignetteSignal requestStopMignetteSignal { get; set; }

		protected override void Start()
		{
			base.Start();
			BuildingViewReference = MignetteBuildingObject.GetComponent<EdwardMinionHandsBuildingViewObject>();
			BuildingViewReference.Reset();
			gameController = base.gameObject.GetComponentInChildren<EdwardMinionHandsGameController>();
			TimeElapsed = 0f;
			TotalEventTime = 45f;
			CollectablePointPool.Clear();
			EdwardMinionHandsBuildingViewObject.CollectableData[] collectablePoolData = BuildingViewReference.CollectablePoolData;
			foreach (EdwardMinionHandsBuildingViewObject.CollectableData collectableData in collectablePoolData)
			{
				for (int j = 0; j < collectableData.numberInPool; j++)
				{
					CollectablePointPool.Add(collectableData.pointValue);
				}
			}
			ToolsWithMinionsList.Clear();
			int num = 0;
			for (num = 0; num < MignetteBuildingObject.GetMignetteMinionCount(); num++)
			{
				TaskingMinionObject childMinion = MignetteBuildingObject.GetChildMinion(num);
				childMinion.Minion.EnableBlobShadow(true);
				if (childMinion.Minion.GetComponent<Collider>() != null)
				{
					childMinion.Minion.GetComponent<Collider>().enabled = true;
				}
				else
				{
					childMinion.Minion.gameObject.AddComponent<CapsuleCollider>();
				}
				GameObject gameObject = Object.Instantiate(BuildingViewReference.CuttingToolPrefab);
				gameObject.transform.SetParent(base.transform, false);
				EdwardMinionHandsCuttingToolViewObject component = gameObject.GetComponent<EdwardMinionHandsCuttingToolViewObject>();
				component.Setup(childMinion.Minion, 2f, this);
				ToolsWithMinionsList.Add(component);
			}
			Transform newTransform = gameController.CameraTransform16x9;
			if (base.mignetteCamera.aspect <= 1.4f)
			{
				newTransform = gameController.CameraTransform4x3;
			}
			RelocateCameraForMignette(newTransform, gameController.FieldOfView, gameController.NearClipPlane, 1f);
			trimEmitter = base.gameObject.AddComponent<CustomFMOD_StudioEventEmitter>();
			trimEmitter.shiftPosition = false;
			trimEmitter.staticSound = false;
			trimEmitter.startEventOnAwake = false;
			trimEmitter.path = fmodService.GetGuid("Play_minion_topiary_trim_01");
			StartCoroutine(IntroSequence());
		}

		private IEnumerator IntroSequence()
		{
			bool lookLeft = true;
			foreach (EdwardMinionHandsCuttingToolViewObject tool2 in ToolsWithMinionsList)
			{
				tool2.StartMinionChat(lookLeft);
				lookLeft = !lookLeft;
			}
			yield return new WaitForSeconds(2f);
			bool everyoneIdle = false;
			while (!everyoneIdle)
			{
				everyoneIdle = true;
				foreach (EdwardMinionHandsCuttingToolViewObject tool in ToolsWithMinionsList)
				{
					if (!tool.IsMinionIdle())
					{
						everyoneIdle = false;
					}
				}
				if (!everyoneIdle)
				{
					yield return new WaitForSeconds(0.1f);
				}
			}
			TrimmingInProgress = true;
			EveryoneCut();
			TimeTillCollectableEmit = BuildingViewReference.TimeBetweenCollectables.Evaluate(TimeElapsed);
		}

		public override void Update()
		{
			if (base.IsPaused)
			{
				return;
			}
			PlayAudioForStateChanges();
			if (hidePolesIfInIdle && ToolsWithMinionsList[0].IsMinionIdleWithPole())
			{
				foreach (EdwardMinionHandsCuttingToolViewObject toolsWithMinions in ToolsWithMinionsList)
				{
					toolsWithMinions.ShowPole(false);
					hidePolesIfInIdle = false;
				}
			}
			base.Update();
			if (!TrimmingInProgress)
			{
				return;
			}
			TimeElapsed += Time.deltaTime;
			if (TimeElapsed >= 45f)
			{
				TimeElapsed = 45f;
				int mignetteData = BuildingViewReference.DisplayRandomTopiary();
				MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(MignetteBuildingObject.ID);
				byInstanceId.MignetteData = mignetteData;
				TrimmingInProgress = false;
				EveryoneCheer();
				hidePolesIfInIdle = true;
				Invoke("ExitMignette", 2f);
			}
			float num = 45f - TimeElapsed;
			TimeTillCollectableEmit -= Time.deltaTime;
			if (!(TimeTillCollectableEmit <= 0f) || !(num >= 5f))
			{
				return;
			}
			currentProgress += 10f;
			GameObject gameObject = Object.Instantiate(BuildingViewReference.CollectablePrefab);
			EdwardMinionHandsCollectableViewObject component = gameObject.GetComponent<EdwardMinionHandsCollectableViewObject>();
			if (CollectablePointPool.Count != 0)
			{
				int num2 = CollectablePointPool[Random.Range(0, CollectablePointPool.Count)];
				CollectablePointPool.Remove(num2);
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = BuildingViewReference.BushLocator.transform.position;
				Vector3 vector = base.mignetteCamera.transform.position - BuildingViewReference.BushLocator.transform.position;
				vector.y = 0f;
				vector.Normalize();
				float num3 = Random.Range(135f, 75f);
				if (Random.Range(0f, 1f) >= 0.5f)
				{
					num3 *= -1f;
				}
				vector = Quaternion.Euler(0f, num3, 0f) * vector;
				float num4 = Random.Range(3.5f, 5f);
				Vector3 targetPos = gameObject.transform.position + vector * num4;
				component.StartCollectable(targetPos, num2, 4f, this);
				globalAudioSignal.Dispatch("Play_dooberSpawn_whistle_01");
				TimeTillCollectableEmit = BuildingViewReference.TimeBetweenCollectables.Evaluate(TimeElapsed);
			}
		}

		public void CollectableHasTimedOut(EdwardMinionHandsCollectableViewObject viewObject)
		{
			if (CollectableDictionary.ContainsKey(viewObject))
			{
				EdwardMinionHandsCuttingToolViewObject edwardMinionHandsCuttingToolViewObject = CollectableDictionary[viewObject];
				edwardMinionHandsCuttingToolViewObject.ClearCollectable();
			}
			Object.Destroy(viewObject.gameObject);
		}

		public void OnInputDown(Vector3 inputPosition)
		{
			Ray ray = base.mignetteCamera.ScreenPointToRay(inputPosition);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 8192))
			{
				EdwardMinionHandsCollectableViewObject component = hitInfo.collider.gameObject.transform.parent.GetComponent<EdwardMinionHandsCollectableViewObject>();
				if (component.WasTapped())
				{
					globalAudioSignal.Dispatch("Play_mignette_collect");
					SendMinionToCollectDoober(component);
				}
			}
		}

		public void CollectableHasBeenCollected(EdwardMinionHandsCollectableViewObject collectableViewObject)
		{
			GameObject gameObject = Object.Instantiate(BuildingViewReference.CollectableGrabbedVfxPrefab);
			gameObject.transform.SetParent(base.transform, false);
			Vector3 position = collectableViewObject.transform.position;
			position.y += 1f;
			gameObject.transform.position = position;
			Object.Destroy(gameObject, 5f);
			int pointValue = collectableViewObject.GetPointValue();
			spawnMignetteDooberSignal.Dispatch(mignetteHUD, collectableViewObject.transform.position, pointValue, true);
			changeScoreSignal.Dispatch(pointValue);
			Object.Destroy(collectableViewObject.gameObject);
		}

		private void SendMinionToCollectDoober(EdwardMinionHandsCollectableViewObject collectableViewObject)
		{
			if (collectableViewObject == null)
			{
				logger.FatalNullArgument(FatalCode.MIGNETTE_BAD_COLLECTABLE_OBJECT);
			}
			else if (!CollectableDictionary.ContainsKey(collectableViewObject))
			{
				EdwardMinionHandsCuttingToolViewObject toolClosestToDoober = GetToolClosestToDoober(collectableViewObject.transform.position);
				if (toolClosestToDoober != null && collectableViewObject != null)
				{
					toolClosestToDoober.GoPickupDoober(collectableViewObject);
					CollectableDictionary.Add(collectableViewObject, toolClosestToDoober);
				}
			}
		}

		private EdwardMinionHandsCuttingToolViewObject GetToolClosestToDoober(Vector3 dooberPos)
		{
			float num = 0f;
			EdwardMinionHandsCuttingToolViewObject edwardMinionHandsCuttingToolViewObject = null;
			foreach (EdwardMinionHandsCuttingToolViewObject toolsWithMinions in ToolsWithMinionsList)
			{
				if (!toolsWithMinions.IsCollecting())
				{
					float num2 = Vector3.Distance(toolsWithMinions.transform.position, dooberPos);
					if (edwardMinionHandsCuttingToolViewObject == null || num2 < num)
					{
						edwardMinionHandsCuttingToolViewObject = toolsWithMinions;
						num = num2;
					}
				}
			}
			return edwardMinionHandsCuttingToolViewObject;
		}

		private void PlayAudioForStateChanges()
		{
			MinionObject myMinionToUpdate = ToolsWithMinionsList[0].myMinionToUpdate;
			AnimatorStateInfo? animatorStateInfo = myMinionToUpdate.GetAnimatorStateInfo(0);
			if (!animatorStateInfo.HasValue)
			{
				return;
			}
			AnimatorStateInfo value = animatorStateInfo.Value;
			if (value.fullPathHash != prevStateNameHash)
			{
				prevStateNameHash = value.fullPathHash;
				if (myMinionToUpdate.IsInAnimatorState(Animator.StringToHash("Base Layer.ChatLeft01")))
				{
					globalAudioSignal.Dispatch("Play_minion_topiary_chatter_01");
				}
				else if (myMinionToUpdate.IsInAnimatorState(Animator.StringToHash("Base Layer.NodRight01")))
				{
					globalAudioSignal.Dispatch("Play_minion_topiary_nod_01");
				}
				else if (myMinionToUpdate.IsInAnimatorState(Animator.StringToHash("Base Layer.NodLeft02")))
				{
					globalAudioSignal.Dispatch("Play_minion_topiary_nod_01");
				}
			}
		}

		private void EveryoneCut()
		{
			trimSoundPlaying = true;
			trimEmitter.Play();
			ToolsWaitingForStateChangeQueue.Clear();
			foreach (EdwardMinionHandsCuttingToolViewObject toolsWithMinions in ToolsWithMinionsList)
			{
				ToolsWaitingForStateChangeQueue.Add(toolsWithMinions);
			}
			StartCoroutine(DequeueCuttingMinions());
		}

		private IEnumerator DequeueCuttingMinions()
		{
			while (ToolsWaitingForStateChangeQueue.Count > 0)
			{
				EdwardMinionHandsCuttingToolViewObject tool = ToolsWaitingForStateChangeQueue[Random.Range(0, ToolsWaitingForStateChangeQueue.Count)];
				ToolsWaitingForStateChangeQueue.Remove(tool);
				tool.StartCutting();
				yield return new WaitForSeconds(0.1f);
			}
			BuildingViewReference.ShakeAnimation.Play();
		}

		private void EveryoneCheer()
		{
			ResetTree();
			globalAudioSignal.Dispatch("Play_mignette_group_cheer");
			foreach (EdwardMinionHandsCuttingToolViewObject toolsWithMinions in ToolsWithMinionsList)
			{
				toolsWithMinions.StopCutting();
				toolsWithMinions.Cheer();
			}
		}

		private void FadeTrimSound()
		{
			if (trimSoundPlaying)
			{
				trimSoundPlaying = false;
				trimEmitter.Fade(1f, 0f, 1f);
			}
		}

		public void ResetTree()
		{
			FadeTrimSound();
			BuildingViewReference.ShakeAnimation.Stop();
			BuildingViewReference.DefaultBush.transform.rotation = Quaternion.identity;
			BuildingViewReference.DefaultBush.transform.localScale = Vector3.one;
		}

		public new void ResetCameraAndStopMignette(bool showScore)
		{
			BuildingViewReference.HasRequestedExit = !showScore;
			base.ResetCameraAndStopMignette(showScore);
		}

		private void ExitMignette()
		{
			requestStopMignetteSignal.Dispatch(true);
		}
	}
}

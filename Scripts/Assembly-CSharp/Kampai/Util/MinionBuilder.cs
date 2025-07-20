using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util.AI;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kampai.Util
{
	public class MinionBuilder : IMinionBuilder
	{
		private Boxed<TargetPerformance> FORCE_LOD;

		private TargetPerformance TargetLOD = TargetPerformance.LOW;

		private bool restart;

		public IKampaiLogger logger = LogManager.GetClassLogger("MinionBuilder") as IKampaiLogger;

		[Inject]
		public PlayLocalAudioSignal audioSignal { get; set; }

		[Inject]
		public StartLoopingAudioSignal startLoopingAudioSignal { get; set; }

		[Inject]
		public StopLocalAudioSignal stopAudioSignal { get; set; }

		[Inject]
		public PlayMinionStateAudioSignal minionStateAudioSignal { get; set; }

		public MinionBuilder()
		{
			SetLOD(TargetLOD);
		}

		public MinionObject BuildMinion(CostumeItemDefinition costume, string animatorStateMachine, GameObject parent, bool showShadow)
		{
			if (FORCE_LOD != null)
			{
				logger.Log(KampaiLogLevel.Warning, "Forced LOD to " + FORCE_LOD);
				restart = false;
				SetLOD(FORCE_LOD.Value);
				FORCE_LOD = null;
			}
			GameObject gameObject = SkinnedMeshAggregator.CreateAggregateObject("NEW_MINION", costume.Skeleton, costume.MeshList, TargetLOD.ToString());
			if (parent != null)
			{
				gameObject.transform.parent = parent.transform;
			}
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;
			RebuildMinion(gameObject);
			Animator component = gameObject.GetComponent<Animator>();
			component.applyRootMotion = false;
			component.runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(animatorStateMachine);
			component.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
			Transform pelvis = gameObject.transform.Find("minion:ROOT/minion:pelvis_jnt");
			return SetupMinionObject(gameObject, pelvis, showShadow);
		}

		public void RebuildMinion(GameObject minion)
		{
			Renderer[] componentsInChildren = minion.GetComponentsInChildren<Renderer>();
			SortedDictionary<string, List<Renderer>> sortedDictionary = new SortedDictionary<string, List<Renderer>>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				string text = ExtractLODKey(componentsInChildren[i]);
				if (text != null)
				{
					if (!sortedDictionary.ContainsKey(text))
					{
						sortedDictionary.Add(text, new List<Renderer>());
					}
					sortedDictionary[text].Add(componentsInChildren[i]);
				}
			}
			float[] lODHeightsArray = GameConstants.GetLODHeightsArray();
			int num = lODHeightsArray.Length - 1;
			float screenRelativeTransitionHeight = lODHeightsArray[num];
			LOD[] array = new LOD[lODHeightsArray.Length];
			using (SortedDictionary<string, List<Renderer>>.Enumerator enumerator = sortedDictionary.GetEnumerator())
			{
				int num2 = 0;
				while (enumerator.MoveNext())
				{
					List<Renderer> value = enumerator.Current.Value;
					Renderer[] array2 = value.ToArray();
					for (int j = 0; j < array2.Length; j++)
					{
						array2[j].shadowCastingMode = ShadowCastingMode.Off;
						array2[j].receiveShadows = false;
					}
					if (num2 < array.Length)
					{
						array[num2] = new LOD(lODHeightsArray[num2], array2);
					}
					else
					{
						LOD lOD = array[num];
						List<Renderer> list = new List<Renderer>(array2);
						if (lOD.renderers != null && lOD.renderers.Length > 0)
						{
							list.AddRange(lOD.renderers);
						}
						array[num] = new LOD(screenRelativeTransitionHeight, list.ToArray());
					}
					num2++;
				}
			}
			LODGroup lODGroup = minion.GetComponent<LODGroup>();
			if (lODGroup == null)
			{
				lODGroup = minion.AddComponent<LODGroup>();
			}
			lODGroup.SetLODs(array);
			lODGroup.RecalculateBounds();
			Transform rootBone = minion.transform.Find("minion:ROOT/minion:pelvis_jnt");
			SkinnedMeshRenderer[] componentsInChildren2 = minion.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
			{
				skinnedMeshRenderer.rootBone = rootBone;
			}
			minion.SetLayerRecursively(8);
		}

		private MinionObject SetupMinionObject(GameObject minion, Transform pelvis, bool showShadow)
		{
			MinionObject minionObject = minion.AddComponent<MinionObject>();
			if (showShadow && GetLOD() != TargetPerformance.LOW && GetLOD() != 0)
			{
				GameObject original = KampaiResources.Load("MinionBlobShadow") as GameObject;
				GameObject gameObject = Object.Instantiate(original);
				gameObject.transform.parent = minion.transform;
				gameObject.GetComponent<MinionBlobShadowView>().SetToTrack(pelvis);
				minionObject.SetBlobShadow(gameObject);
			}
			minion.AddComponent<SteerToAvoidCollisions>();
			minion.AddComponent<SteerToAvoidEnvironment>();
			minion.AddComponent<SteerToTether>();
			minion.AddComponent<SteerMinionToWander>();
			SteerCharacterToSeek steerCharacterToSeek = minion.AddComponent<SteerCharacterToSeek>();
			steerCharacterToSeek.enabled = false;
			steerCharacterToSeek.Threshold = 0.1f;
			Agent agent = minion.GetComponent<Agent>();
			if (agent == null)
			{
				agent = minion.AddComponent<Agent>();
			}
			agent.Radius = 0.5f;
			agent.Mass = 1f;
			agent.MaxForce = 8f;
			agent.MaxSpeed = 1f;
			AnimEventHandler animEventHandler = minion.AddComponent<AnimEventHandler>();
			animEventHandler.Init(minionObject, minionObject.localAudioEmitter, audioSignal, stopAudioSignal, minionStateAudioSignal, startLoopingAudioSignal);
			return minionObject;
		}

		public TargetPerformance GetLOD()
		{
			return TargetLOD;
		}

		public void SetLOD(TargetPerformance targetPerformance)
		{
			if (targetPerformance != TargetPerformance.UNKNOWN && targetPerformance != TargetPerformance.UNSUPPORTED)
			{
				TargetLOD = targetPerformance;
			}
			else
			{
				TargetLOD = TargetPerformance.LOW;
				logger.Error("Unsupported/Unknown device: {0}, setting to LOW", targetPerformance);
			}
			if (restart)
			{
				FORCE_LOD = new Boxed<TargetPerformance>(targetPerformance);
			}
			restart = true;
		}

		private string ExtractLODKey(Renderer renderer)
		{
			string result = null;
			string name = renderer.name;
			if (name.Length > 4)
			{
				string text = name.Substring(name.Length - 4);
				if (text.Contains("LOD"))
				{
					result = text;
				}
			}
			return result;
		}
	}
}

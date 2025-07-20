using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class StageBuildingObject : AnimatingBuildingObject
	{
		private const string Base_Layer_StageIdle = "Base Layer.StageIdle";

		private const string BOOL_HIDE_MIC = "HideMic";

		private Transform stageTransform;

		private StageBuildingSetting setting;

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			if (routes != null && routes.Length > 0)
			{
				stageTransform = routes[0];
			}
			if (stageTransform == null)
			{
				stageTransform = base.transform;
			}
			BuildingState state = building.State;
			if (state != BuildingState.Inaccessible && state != 0 && state != BuildingState.Broken)
			{
				setting = base.gameObject.GetComponent<StageBuildingSetting>();
				if (setting == null)
				{
					logger.Error("Stage state is {0}. StageBuildingSetting is null.", state);
				}
				else
				{
					UpdateStageState(state);
				}
			}
		}

		public Transform GetStageTransform()
		{
			return stageTransform;
		}

		public void UpdateStageState(BuildingState stageState)
		{
			if (setting == null)
			{
				logger.Error("StageBuildingSetting is null");
				return;
			}
			switch (stageState)
			{
			case BuildingState.SocialAvailable:
				setting.SocialAvailableObject.SetActive(true);
				setting.SocialCompleteObject.SetActive(false);
				setting.LeftTorchVFX.Stop();
				setting.RightTorchVFX.Stop();
				break;
			case BuildingState.SocialComplete:
				setting.SocialAvailableObject.SetActive(true);
				setting.SocialCompleteObject.SetActive(true);
				setting.LeftTorchVFX.Play();
				setting.RightTorchVFX.Play();
				break;
			default:
				setting.SocialAvailableObject.SetActive(false);
				setting.SocialCompleteObject.SetActive(false);
				setting.LeftTorchVFX.Stop();
				setting.RightTorchVFX.Stop();
				break;
			}
		}

		public void SetSpinMic()
		{
			SetAnimTrigger("OnSpinMic");
		}

		public void SetHideMic(bool enable)
		{
			SetAnimBool("HideMic", enable);
		}

		public void PerformanceStarts()
		{
			EnqueueAction(new SetAnimatorArgumentsAction(this, logger, "HideMic", false), true);
			EnqueueAction(new PlayMecanimStateAction(this, Animator.StringToHash("Base Layer.StageIdle"), logger));
		}
	}
}

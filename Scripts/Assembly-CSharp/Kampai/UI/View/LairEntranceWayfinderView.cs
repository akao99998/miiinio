using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class LairEntranceWayfinderView : AbstractWayFinderView
	{
		private GoTween pulseTween;

		protected override string UIName
		{
			get
			{
				return "LairEntranceWayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.KevinLairIcon;
			}
		}

		protected override bool OnCanUpdate()
		{
			if (zoomCameraModel.ZoomedIn)
			{
				return false;
			}
			return true;
		}

		internal void StartPulse()
		{
			Vector3 originalScale = Vector3.one;
			pulseTween = TweenUtil.Throb(base.gameObject.transform, 0.85f, 0.5f, out originalScale);
		}

		internal void StopPulse()
		{
			if (pulseTween != null)
			{
				pulseTween.destroy();
			}
		}

		internal void TaskUpdated(MasterPlanComponent component)
		{
			bool flag = true;
			bool flag2 = false;
			for (int i = 0; i < component.tasks.Count; i++)
			{
				MasterPlanComponentTask masterPlanComponentTask = component.tasks[i];
				if (masterPlanComponentTask.isHarvestable)
				{
					flag2 = true;
				}
				if (!masterPlanComponentTask.isComplete)
				{
					flag = false;
				}
			}
			if (flag && component.State <= MasterPlanComponentState.TasksComplete)
			{
				SetBuildReadyIcon();
			}
			else if (flag2)
			{
				UpdateIcon(wayFinderDefinition.MasterPlanComponentTaskCompleteIcon);
			}
			else
			{
				ResetDefaultIcon();
			}
		}

		internal void SetBuildReadyIcon()
		{
			UpdateIcon(wayFinderDefinition.MasterPlanComponentCompleteIcon);
		}

		internal void ResetDefaultIcon()
		{
			UpdateIcon(wayFinderDefinition.KevinLairIcon);
		}
	}
}

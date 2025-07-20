using Kampai.Game;
using UnityEngine;

namespace Kampai.UI.View
{
	public class VolcanoLairWayfinderView : AbstractWayFinderView
	{
		protected override string UIName
		{
			get
			{
				return "VolcanoLairWayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.KevinLairIcon;
			}
		}

		internal void SetOffset()
		{
			UIOffset = new Vector3(0f, 0.5f, 0f);
		}

		protected override bool OnCanUpdate()
		{
			if (zoomCameraModel.ZoomedIn)
			{
				return false;
			}
			return true;
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

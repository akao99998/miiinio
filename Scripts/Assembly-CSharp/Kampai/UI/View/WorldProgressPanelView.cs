using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class WorldProgressPanelView : KampaiView
	{
		private Dictionary<int, ProgressBarView> trackedProgressBar;

		private MoveBuildingMenuView menuView;

		private IKampaiLogger logger;

		internal void Init(IKampaiLogger logger)
		{
			this.logger = logger;
			menuView = null;
			trackedProgressBar = new Dictionary<int, ProgressBarView>();
		}

		internal void Cleanup()
		{
			if (trackedProgressBar == null)
			{
				return;
			}
			foreach (ProgressBarView value in trackedProgressBar.Values)
			{
				if (value != null)
				{
					Object.Destroy(value.gameObject);
				}
			}
			trackedProgressBar.Clear();
		}

		internal GameObject CreateOrUpdateMoveBuildingMenu(MoveBuildingSetting moveBuildingSettings)
		{
			if (menuView == null)
			{
				menuView = WorldToGlassUIBuilder.Build<MoveBuildingMenuView>("screen_MoveBuilding", base.transform, moveBuildingSettings, logger);
			}
			else
			{
				WorldToGlassUIModal component = menuView.gameObject.GetComponent<WorldToGlassUIModal>();
				component.Settings = moveBuildingSettings;
				menuView.ReloadModal(component);
			}
			menuView.SetButtonState(moveBuildingSettings.Mask);
			return menuView.gameObject;
		}

		internal void RemoveMoveBuildingMenu()
		{
			if (menuView != null)
			{
				Object.Destroy(menuView.gameObject);
				menuView = null;
			}
		}

		internal void CreateProgressBar(ProgressBarSettings progressBarSettings)
		{
			int trackedId = progressBarSettings.TrackedId;
			logger.Info("Creating Progress Bar with id: {0}", trackedId);
			ProgressBarView progressBarView = null;
			if ((progressBarView = GetProgressBar(trackedId)) != null)
			{
				logger.Info("Progress Bar with id: {0} already exists, ignoring", trackedId);
			}
			else
			{
				progressBarView = WorldToGlassUIBuilder.Build<ProgressBarView>("cmp_BuildingProgress", base.transform, progressBarSettings, logger);
				trackedProgressBar.Add(trackedId, progressBarView);
			}
		}

		private bool ContainsProgressBar(int trackedId)
		{
			if (trackedProgressBar != null && trackedProgressBar.ContainsKey(trackedId))
			{
				return true;
			}
			return false;
		}

		private ProgressBarView GetProgressBar(int trackedId)
		{
			if (ContainsProgressBar(trackedId))
			{
				return trackedProgressBar[trackedId];
			}
			return null;
		}

		internal void RemoveProgressBar(int trackedId)
		{
			logger.Info("Removing Progress Bar with id: {0}", trackedId);
			if (ContainsProgressBar(trackedId))
			{
				ProgressBarView progressBarView = trackedProgressBar[trackedId];
				trackedProgressBar.Remove(trackedId);
				Object.Destroy(progressBarView.gameObject);
			}
			else
			{
				logger.Warning("Progress Bar with id: {0} will not be removed since it doesn't exist!", trackedId);
			}
		}
	}
}

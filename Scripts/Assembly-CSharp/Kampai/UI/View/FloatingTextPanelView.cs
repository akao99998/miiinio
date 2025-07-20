using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class FloatingTextPanelView : KampaiView
	{
		private Dictionary<int, FloatingTextView> trackedFloatingTexts;

		private IKampaiLogger logger;

		internal void Init(IKampaiLogger logger)
		{
			this.logger = logger;
			trackedFloatingTexts = new Dictionary<int, FloatingTextView>();
		}

		internal void Cleanup()
		{
			if (trackedFloatingTexts == null)
			{
				return;
			}
			foreach (FloatingTextView value in trackedFloatingTexts.Values)
			{
				if (value != null)
				{
					Object.Destroy(value.gameObject);
				}
			}
			trackedFloatingTexts.Clear();
		}

		internal void CreateFloatingText(FloatingTextSettings settings)
		{
			int trackedId = settings.TrackedId;
			logger.Info("Creating Text WayFinder with id: {0}", trackedId);
			FloatingTextView floatingTextView = null;
			if ((floatingTextView = GetFloatingText(trackedId)) != null)
			{
				logger.Info("Text WayFinder with id: {0} already exists, ignoring", trackedId);
				return;
			}
			floatingTextView = WorldToGlassUIBuilder.Build<FloatingTextView>("cmp_FloatingText", base.transform, settings, logger);
			if (settings.heightOverrideActive)
			{
				floatingTextView.SetHeight(settings.height);
			}
			trackedFloatingTexts.Add(trackedId, floatingTextView);
		}

		internal void RemoveFloatingText(int trackedId)
		{
			logger.Info("Removing Text WayFinder with id: {0}", trackedId);
			if (ContainsFloatingText(trackedId))
			{
				FloatingTextView floatingTextView = trackedFloatingTexts[trackedId];
				trackedFloatingTexts.Remove(trackedId);
				Object.Destroy(floatingTextView.gameObject);
			}
			else
			{
				logger.Warning("Text WayFinder with id: {0} will not be removed since it doesn't exist!", trackedId);
			}
		}

		internal void ToggleAllFloatingText(bool show)
		{
			if (trackedFloatingTexts == null || trackedFloatingTexts.Count == 0)
			{
				return;
			}
			foreach (FloatingTextView value in trackedFloatingTexts.Values)
			{
				value.SetForceHide(!show);
			}
		}

		private FloatingTextView GetFloatingText(int trackedId)
		{
			if (ContainsFloatingText(trackedId))
			{
				return trackedFloatingTexts[trackedId];
			}
			return null;
		}

		private bool ContainsFloatingText(int trackedId)
		{
			if (trackedFloatingTexts != null && trackedFloatingTexts.ContainsKey(trackedId))
			{
				return true;
			}
			return false;
		}
	}
}

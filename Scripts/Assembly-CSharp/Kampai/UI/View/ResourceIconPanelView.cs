using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class ResourceIconPanelView : KampaiView
	{
		private Dictionary<int, Dictionary<int, ResourceIconView>> trackedResourceIcons;

		private IKampaiLogger logger;

		private bool isForceHideEnabled;

		internal void Init(IKampaiLogger logger)
		{
			this.logger = logger;
			trackedResourceIcons = new Dictionary<int, Dictionary<int, ResourceIconView>>();
		}

		internal void Cleanup()
		{
			if (trackedResourceIcons == null)
			{
				return;
			}
			foreach (Dictionary<int, ResourceIconView> value in trackedResourceIcons.Values)
			{
				if (value == null)
				{
					continue;
				}
				foreach (ResourceIconView value2 in value.Values)
				{
					if (value2 != null)
					{
						Object.Destroy(value2.gameObject);
					}
				}
			}
			trackedResourceIcons.Clear();
		}

		internal void CreateResourceIcon(ResourceIconSettings resourceIconSettings)
		{
			int trackedId = resourceIconSettings.TrackedId;
			int itemDefId = resourceIconSettings.ItemDefId;
			logger.Info("Creating Resource Icon with id: {0} and itemDefId: {1}", trackedId, itemDefId);
			ResourceIconView resourceIconView = null;
			if ((resourceIconView = GetResourceIcon(trackedId, itemDefId)) != null)
			{
				if (base.gameObject.activeInHierarchy)
				{
					resourceIconView.UpdateIconCount(resourceIconSettings.Count);
					logger.Info("Resource Icon with id: {0} and itemDefId: {1} already exists, ignoring create but updating count", trackedId, itemDefId);
				}
				else
				{
					WorldToGlassUIModal component = resourceIconView.GetComponent<WorldToGlassUIModal>();
					component.Settings = resourceIconSettings;
				}
				return;
			}
			resourceIconView = WorldToGlassUIBuilder.Build<ResourceIconView>("cmp_HarvestIcon", base.transform, resourceIconSettings, logger);
			WorldToGlassUIModal component2 = resourceIconView.GetComponent<WorldToGlassUIModal>();
			logger.Info("Got Resource Icon Modal " + component2.name);
			if (ContainsResourceIcon(trackedId))
			{
				trackedResourceIcons[trackedId].Add(itemDefId, resourceIconView);
				Dictionary<int, ResourceIconView>.ValueCollection values = trackedResourceIcons[trackedId].Values;
				UpdateIconIndexes(values);
			}
			else
			{
				Dictionary<int, ResourceIconView> dictionary = new Dictionary<int, ResourceIconView>();
				dictionary.Add(itemDefId, resourceIconView);
				trackedResourceIcons.Add(trackedId, dictionary);
			}
			resourceIconView.SetForceHide(isForceHideEnabled);
		}

		private void UpdateIconIndexes(Dictionary<int, ResourceIconView>.ValueCollection resourceIconViews)
		{
			int count = resourceIconViews.Count;
			float num = -(count / 2);
			if (count % 2 == 0)
			{
				num += 0.4f;
			}
			int num2 = 0;
			Dictionary<int, ResourceIconView>.ValueCollection.Enumerator enumerator = resourceIconViews.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ResourceIconView current = enumerator.Current;
					current.UpdateIconIndex(num + (float)num2);
					num2++;
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}

		private bool ContainsResourceIcon(int trackedId)
		{
			if (trackedResourceIcons != null && trackedResourceIcons.ContainsKey(trackedId))
			{
				return true;
			}
			return false;
		}

		private bool ContainsResourceIcon(int trackedId, int itemDefId)
		{
			if (ContainsResourceIcon(trackedId))
			{
				Dictionary<int, ResourceIconView> dictionary = trackedResourceIcons[trackedId];
				if (dictionary != null && dictionary.ContainsKey(itemDefId))
				{
					return true;
				}
			}
			return false;
		}

		internal void UpdateResourceIconCount(int trackedId, int itemId, int count)
		{
			ResourceIconView resourceIcon = GetResourceIcon(trackedId, itemId);
			if (resourceIcon != null)
			{
				resourceIcon.UpdateIconCount(count);
				return;
			}
			logger.Warning("Could not find resource icon with id: {0} and itemId: {1} ", trackedId, itemId);
		}

		private Dictionary<int, ResourceIconView> GetResourceIcon(int trackedId)
		{
			if (ContainsResourceIcon(trackedId))
			{
				return trackedResourceIcons[trackedId];
			}
			return null;
		}

		private ResourceIconView GetResourceIcon(int trackedId, int itemDefId)
		{
			Dictionary<int, ResourceIconView> resourceIcon = GetResourceIcon(trackedId);
			if (resourceIcon != null && resourceIcon.ContainsKey(itemDefId))
			{
				return resourceIcon[itemDefId];
			}
			return null;
		}

		internal void RemoveResourceIcon(int trackedId)
		{
			logger.Info("Removing Resource Icon with id: {0}", trackedId);
			if (ContainsResourceIcon(trackedId))
			{
				Dictionary<int, ResourceIconView> dictionary = trackedResourceIcons[trackedId];
				{
					foreach (int key in dictionary.Keys)
					{
						RemoveResourceIcon(trackedId, key);
					}
					return;
				}
			}
			logger.Warning("Resource Icon with id: {0} will not be removed since it doesn't exist!", trackedId);
		}

		internal void RemoveResourceIcon(int trackedId, int itemDefId)
		{
			logger.Info("Removing Resource Icon with id: {0} and itemDefId: {1}", trackedId, itemDefId);
			if (ContainsResourceIcon(trackedId, itemDefId))
			{
				Dictionary<int, ResourceIconView> dictionary = trackedResourceIcons[trackedId];
				ResourceIconView resourceIconView = dictionary[itemDefId];
				resourceIconView.Close();
				dictionary.Remove(itemDefId);
				if (dictionary.Count == 0)
				{
					trackedResourceIcons.Remove(trackedId);
				}
				else
				{
					UpdateIconIndexes(dictionary.Values);
				}
			}
			else
			{
				logger.Warning("Ignoring remove Resource Icon with id: {0} and itemDefId: {1} since it doesn't exist", trackedId, itemDefId);
			}
		}

		private void SetForceHideForAllResourceIcons()
		{
			foreach (Dictionary<int, ResourceIconView> value in trackedResourceIcons.Values)
			{
				foreach (ResourceIconView value2 in value.Values)
				{
					value2.SetForceHide(isForceHideEnabled);
				}
			}
		}

		internal void HideAllResourceIcons()
		{
			isForceHideEnabled = true;
			SetForceHideForAllResourceIcons();
		}

		internal void ShowAllResourceIcons()
		{
			isForceHideEnabled = false;
			SetForceHideForAllResourceIcons();
		}
	}
}

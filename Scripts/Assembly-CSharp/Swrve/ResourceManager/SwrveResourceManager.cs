using System.Collections.Generic;

namespace Swrve.ResourceManager
{
	public class SwrveResourceManager
	{
		public Dictionary<string, SwrveResource> UserResources;

		public void SetResourcesFromJSON(Dictionary<string, Dictionary<string, string>> userResources)
		{
			Dictionary<string, SwrveResource> dictionary = new Dictionary<string, SwrveResource>();
			foreach (string key in userResources.Keys)
			{
				dictionary[key] = new SwrveResource(userResources[key]);
			}
			UserResources = dictionary;
		}

		public SwrveResource GetResource(string resourceId)
		{
			if (UserResources != null)
			{
				if (UserResources.ContainsKey(resourceId))
				{
					return UserResources[resourceId];
				}
			}
			else
			{
				SwrveLog.LogWarning(string.Format("SwrveResourceManager::GetResource('{0}'): Resources are not available yet.", resourceId));
			}
			return null;
		}

		public T GetResourceAttribute<T>(string resourceId, string attributeName, T defaultValue)
		{
			SwrveResource resource = GetResource(resourceId);
			if (resource != null)
			{
				return resource.GetAttribute(attributeName, defaultValue);
			}
			return defaultValue;
		}
	}
}

using UnityEngine;

public class ResourceService : IResourceService
{
	public Object Load(string path)
	{
		return Resources.Load(path);
	}

	public string LoadText(string path)
	{
		return (Resources.Load(path) as TextAsset).text;
	}
}

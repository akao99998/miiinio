using UnityEngine;

public interface IResourceService
{
	Object Load(string path);

	string LoadText(string path);
}

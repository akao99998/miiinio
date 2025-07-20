using UnityEngine;

public static class LayerUtils
{
	public static void SetLayerRecursively(this GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform item in obj.transform)
		{
			item.gameObject.SetLayerRecursively(layer);
		}
	}
}

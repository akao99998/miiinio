using UnityEngine;

public static class TransformUtils
{
	public static GameObject FindChild(this GameObject gameObject, string name)
	{
		GameObject gameObject2 = null;
		foreach (Transform item in gameObject.transform)
		{
			if (gameObject2 != null)
			{
				break;
			}
			if (item.gameObject.name.Equals(name))
			{
				gameObject2 = item.gameObject;
			}
			else if (item.childCount > 0)
			{
				gameObject2 = item.gameObject.FindChild(name);
			}
		}
		return gameObject2;
	}

	public static T GetComponentTypeInParent<T>(this Transform gameObject) where T : Component
	{
		if (gameObject == null || gameObject.transform == null)
		{
			return (T)null;
		}
		Transform parent = gameObject.transform.parent;
		T val = (T)null;
		while (parent != null)
		{
			val = parent.GetComponent<T>();
			if (val != null)
			{
				return val;
			}
			parent = parent.parent;
		}
		return val;
	}

	public static void ResetLocal(this Transform transform)
	{
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}
}

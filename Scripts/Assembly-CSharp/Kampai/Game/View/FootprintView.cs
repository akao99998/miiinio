using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class FootprintView : KampaiView
	{
		private Transform cachedTransform;

		private GameObject green;

		private GameObject red;

		public void Init()
		{
			cachedTransform = base.transform;
			green = base.transform.Find("Green").gameObject;
			red = base.transform.Find("Red").gameObject;
			green.gameObject.SetLayerRecursively(14);
			red.gameObject.SetLayerRecursively(14);
		}

		public void ToggleFootprint(bool enable)
		{
			base.gameObject.SetActive(enable);
		}

		public void ParentFootprint(ActionableObject parentObject, Transform parentTransform, int width, int height)
		{
			if (null != parentObject)
			{
				parentObject.gameObject.SetLayerRecursively(14);
			}
			cachedTransform.parent = parentTransform;
			cachedTransform.localPosition = new Vector3((float)width / 2f - 0.5f, 0f, (float)(-height) / 2f + 0.5f);
			cachedTransform.position = new Vector3(cachedTransform.position.x, 0f, cachedTransform.position.z);
			cachedTransform.localScale = new Vector3(width, height, 1f);
		}

		public void Reset()
		{
			cachedTransform.parent = null;
			cachedTransform.position = Vector3.zero;
			cachedTransform.localScale = Vector3.one;
		}

		public void UpdateFootprint(bool valid)
		{
			green.SetActive(valid);
			red.SetActive(!valid);
		}
	}
}

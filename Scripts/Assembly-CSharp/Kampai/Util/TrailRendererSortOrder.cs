using UnityEngine;

namespace Kampai.Util
{
	public class TrailRendererSortOrder : MonoBehaviour
	{
		public int SortingLayer = -1;

		private void Start()
		{
			TrailRenderer component = base.gameObject.GetComponent<TrailRenderer>();
			if (component != null)
			{
				component.sortingOrder = SortingLayer;
			}
		}
	}
}

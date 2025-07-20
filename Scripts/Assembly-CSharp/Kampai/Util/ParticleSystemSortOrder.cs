using UnityEngine;

namespace Kampai.Util
{
	public class ParticleSystemSortOrder : MonoBehaviour
	{
		public int SortingLayer = 1;

		private void Awake()
		{
			if (GetComponent<ParticleSystem>() != null && GetComponent<ParticleSystem>().GetComponent<Renderer>() != null)
			{
				GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingOrder = SortingLayer;
			}
		}
	}
}

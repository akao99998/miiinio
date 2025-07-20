using System.Collections;
using UnityEngine;

namespace Kampai.Util
{
	public class DestroyMeScript : MonoBehaviour
	{
		public float DestroyTime;

		private void Start()
		{
			StartCoroutine("DestroyMe");
		}

		private IEnumerator DestroyMe()
		{
			yield return new WaitForSeconds(DestroyTime);
			Object.Destroy(base.gameObject);
		}
	}
}

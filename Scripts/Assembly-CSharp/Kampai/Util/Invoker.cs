using UnityEngine;

namespace Kampai.Util
{
	public class Invoker : MonoBehaviour
	{
		private InvokerService invokerService;

		private bool isInitialized;

		public void Initialize(InvokerService invokerService)
		{
			this.invokerService = invokerService;
			isInitialized = true;
		}

		private void Update()
		{
			if (isInitialized)
			{
				invokerService.Update();
			}
		}
	}
}

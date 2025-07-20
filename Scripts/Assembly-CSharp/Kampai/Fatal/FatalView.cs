using Kampai.Common;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Fatal
{
	public class FatalView : KampaiView
	{
		[Inject]
		public ReInitializeGameSignal reInitializeGameSignal { get; set; }

		private void OnApplicationPause(bool isPausing)
		{
			if (!isPausing && reInitializeGameSignal != null)
			{
				reInitializeGameSignal.Dispatch(string.Empty);
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				reInitializeGameSignal.Dispatch(string.Empty);
			}
		}
	}
}

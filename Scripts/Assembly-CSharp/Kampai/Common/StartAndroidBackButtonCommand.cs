using System.Collections;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class StartAndroidBackButtonCommand : Command
	{
		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public AndroidBackButtonSignal androidBackButtonSignal { get; set; }

		public override void Execute()
		{
			routineRunner.StartCoroutine(Update());
		}

		private IEnumerator Update()
		{
			while (true)
			{
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					androidBackButtonSignal.Dispatch();
				}
				yield return null;
			}
		}
	}
}

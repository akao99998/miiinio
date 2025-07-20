using System.Collections;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class LoadToggleCommand : Command
	{
		[Inject]
		public IRoutineRunner RoutineRunner { get; set; }

		[Inject]
		public InitToggleSignal InitToggleSignal { get; set; }

		[Inject]
		public bool isToggleActive { get; set; }

		[Inject]
		public int InstanceId { get; set; }

		[Inject]
		public string LabelText { get; set; }

		public override void Execute()
		{
			GameObject original = Resources.Load<GameObject>("Toggle");
			GameObject toggleInstance = Object.Instantiate(original);
			RoutineRunner.StartCoroutine(InitializeToggle(toggleInstance));
		}

		private IEnumerator InitializeToggle(GameObject toggleInstance)
		{
			yield return new WaitForEndOfFrame();
			ToggleMediator mediator = toggleInstance.GetComponent<ToggleMediator>();
			mediator.InitializeToggle(InstanceId, LabelText);
			toggleInstance.GetComponent<Toggle>().isOn = isToggleActive;
		}
	}
}

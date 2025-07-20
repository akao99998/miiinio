using Kampai.Main;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Util.AnimatorStateInfo
{
	public class UnloadAnimatorStateInfoCommand : Command
	{
		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		public override void Execute()
		{
			GameObject obj = glassCanvas.FindChild("Animator State Views");
			Object.Destroy(obj);
		}
	}
}

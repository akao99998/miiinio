using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowActivitySpinnerCommand : Command
	{
		private GameObject go;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public bool show { get; set; }

		[Inject]
		public Vector3 inputPos { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		public override void Execute()
		{
			if (show)
			{
				go = guiService.Execute(GUIOperation.Load, "Spinner_group");
				go.GetComponent<CanvasGroup>().blocksRaycasts = false;
				go.transform.position = positionService.GetPositionData(inputPos).WorldPositionInUI;
			}
			else
			{
				guiService.Execute(GUIOperation.Unload, "Spinner_group");
			}
		}
	}
}

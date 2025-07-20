using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

namespace Kampai.Game.Mignette.AlligatorSkiing
{
	public class AlligatorSkiingMignetteSetupCommand : SetupMignetteManagerViewCommand
	{
		public override void Execute()
		{
			AlligatorSkiingMignetteManagerView alligatorSkiingMignetteManagerView = CreateManagerView<AlligatorSkiingMignetteManagerView>("AlligatorSkiingMignetteManagerView");
			base.contextView.transform.position = alligatorSkiingMignetteManagerView.MignetteBuildingObject.transform.position;
			InitializeChildObjects(alligatorSkiingMignetteManagerView);
			Shader.Find("Kampai/Standard/Hidden");
		}
	}
}

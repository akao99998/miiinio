using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class InitBuildingObjectCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("InitBuildingObjectCommand") as IKampaiLogger;

		[Inject]
		public BuildingObject buildingObject { get; set; }

		[Inject]
		public Building building { get; set; }

		[Inject]
		public Dictionary<string, RuntimeAnimatorController> animatorControllers { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			ActionableObject component = buildingObject.GetComponent<ActionableObject>();
			if (component != null)
			{
				base.injectionBinder.injector.Inject(component, false);
			}
			buildingObject.Init(building, logger, animatorControllers, definitionService);
		}
	}
}

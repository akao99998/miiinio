using System.Collections;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	internal sealed class SetupBuildingManagerCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupBuildingManagerCommand") as IKampaiLogger;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ISpecialEventService specialEventService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void Execute()
		{
			GameObject gameObject = new GameObject("Buildings");
			BuildingManagerView buildingManagerView = gameObject.AddComponent<BuildingManagerView>();
			buildingManagerView.Init(logger, definitionService, masterPlanService, specialEventService.IsSpecialEventActive());
			base.injectionBinder.Bind<GameObject>().ToValue(gameObject).ToName(GameElement.BUILDING_MANAGER)
				.CrossContext();
			gameObject.transform.parent = contextView.transform;
			logger.Debug("SetupBuildingManagerCommand: Building Manager created.");
			routineRunner.StartCoroutine(WaitAFrame());
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			base.injectionBinder.GetInstance<PopulateBuildingSignal>().Dispatch();
		}
	}
}

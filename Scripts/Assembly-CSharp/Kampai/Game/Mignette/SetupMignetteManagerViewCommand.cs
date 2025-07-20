using Elevation.Logging;
using Kampai.Game.Mignette.View;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game.Mignette
{
	public abstract class SetupMignetteManagerViewCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("SetupMignetteManagerViewCommand") as IKampaiLogger;

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public MignetteGameModel mignetteModel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		protected T CreateManagerView<T>(string viewName) where T : MignetteManagerView
		{
			GameObject gameObject = new GameObject(viewName);
			gameObject.transform.parent = contextView.transform;
			T val = gameObject.AddComponent<T>();
			val.MignetteBuildingObject = GetMignetteBuildingObject();
			return val;
		}

		protected void InitializeChildObjects(MignetteManagerView managerView)
		{
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(managerView.MignetteBuildingObject.ID);
			MignetteBuildingDefinition mignetteBuildingDefinition = byInstanceId.MignetteBuildingDefinition;
			Transform transform = managerView.transform;
			if (mignetteBuildingDefinition.ChildObjects == null)
			{
				return;
			}
			foreach (MignetteChildObjectDefinition childObject in mignetteBuildingDefinition.ChildObjects)
			{
				GameObject gameObject = Object.Instantiate(KampaiResources.Load<GameObject>(childObject.Prefab), transform.position, Quaternion.identity) as GameObject;
				Transform transform2 = gameObject.transform;
				transform2.parent = transform;
				if (childObject.IsLocal)
				{
					transform2.localPosition = childObject.Position;
					transform2.localRotation = Quaternion.Euler(0f, childObject.Rotation, 0f);
				}
				else
				{
					transform2.position = childObject.Position;
					transform2.rotation = Quaternion.Euler(0f, childObject.Rotation, 0f);
				}
			}
		}

		protected MignetteBuildingObject GetMignetteBuildingObject()
		{
			GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER);
			BuildingManagerView component = instance.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(mignetteModel.BuildingId);
			MignetteBuildingObject mignetteBuildingObject = buildingObject as MignetteBuildingObject;
			if (mignetteBuildingObject == null)
			{
				logger.Fatal(FatalCode.MIGNETTE_BAD_BUILDING);
			}
			return mignetteBuildingObject;
		}
	}
}

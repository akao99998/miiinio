using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.pool.api;

namespace Kampai.UI.View
{
	public class SpawnDooberCommand : DooberCommand, IPoolable, IFastPooledCommand<Vector3, DestinationType, int, bool>, IFastPooledCommandBase
	{
		private DestinationType type;

		private float iconWidth = 70f;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject(UIElement.HUD)]
		public GameObject hud { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject(MainElement.UI_DOOBER_CANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public PeekHUDSignal peekHUDSignal { get; set; }

		[Inject]
		public PeekStoreSignal peekStoreSignal { get; set; }

		[Inject]
		public TokenDooberHasBeenSpawnedSignal tokenDooberHasBeenSpawnedSignal { get; set; }

		public void Execute(Vector3 iconPos, DestinationType _type, int itemDefinitionID, bool fromWorldCanvas)
		{
			base.fromWorldCanvas = fromWorldCanvas;
			iconPosition = iconPos;
			type = _type;
			itemDefinitionId = itemDefinitionID;
			Transform transform = glassCanvas.transform;
			GameObject gameObject = CreateTweenObject(transform);
			KampaiImage component = gameObject.GetComponent<KampaiImage>();
			Vector3 destination = Vector3.zero;
			peekHUDSignal.Dispatch(3f);
			peekStoreSignal.Dispatch(3f);
			switch (type)
			{
			case DestinationType.XP:
				destination = GetXPGlassPosition();
				itemDefinitionId = 2;
				break;
			case DestinationType.GRIND:
				destination = GetGrindGlassPosition();
				itemDefinitionId = 0;
				break;
			case DestinationType.PREMIUM:
				destination = GetPremiumGlassPosition();
				itemDefinitionId = 1;
				break;
			case DestinationType.MINIONS:
				destination = GetInspirationGlassPosition();
				itemDefinitionId = 5;
				break;
			case DestinationType.STORAGE:
			case DestinationType.STORAGE_POPULATION_GOAL:
				destination = GetStorageGlassPosition();
				break;
			case DestinationType.BUFF:
				destination = GetDiscoGlassPosition();
				break;
			case DestinationType.STICKER:
				destination = GetTikiHutGlassPosition();
				gameContext.injectionBinder.GetInstance<ToggleStickerbookGlowSignal>().Dispatch(true);
				localPersistence.PutDataPlayer("StickerbookGlow", "Enable");
				break;
			case DestinationType.STORE:
				destination = GetStoreGlassPosition();
				break;
			case DestinationType.MINION_LEVEL_TOKEN:
				tokenDooberHasBeenSpawnedSignal.Dispatch();
				destination = GetMinionLevelTokenGlassPosition();
				break;
			case DestinationType.MYSTERY_BOX:
				destination = DetermineMysteryDestination();
				break;
			case DestinationType.TIMER_POPULATION_GOAL:
				destination = GetMiscGlassPosition();
				break;
			}
			component.sprite = GetIconFromDefinitionID(itemDefinitionId);
			component.material = KampaiResources.Load<Material>("CircleIconAlphaMaskMat");
			component.materialForRendering.renderQueue = 3001;
			component.maskSprite = GetMaskFromDefinitionId(itemDefinitionId);
			destination.z = transform.position.z;
			TweenToDestination(gameObject, destination, 1f, type);
		}

		private Vector3 DetermineMysteryDestination()
		{
			if (itemDefinitionId == 1)
			{
				return GetPremiumGlassPosition();
			}
			return GetStorageGlassPosition();
		}

		private Vector3 GetXPGlassPosition()
		{
			if (base.dooberModel.XPGlassPosition == Vector3.zero)
			{
				base.dooberModel.XPGlassPosition = hud.GetComponent<HUDView>().PointsPanel.position;
			}
			return base.dooberModel.XPGlassPosition;
		}

		private Vector3 GetGrindGlassPosition()
		{
			if (base.dooberModel.GrindGlassPosition == Vector3.zero)
			{
				base.dooberModel.GrindGlassPosition = hud.transform.Find("group_Store_Element/group_Currency_Grind/icn_Currency_Grind").position;
			}
			return base.dooberModel.GrindGlassPosition;
		}

		private Vector3 GetPremiumGlassPosition()
		{
			if (base.dooberModel.PremiumGlassPosition == Vector3.zero)
			{
				base.dooberModel.PremiumGlassPosition = hud.transform.Find("group_Store_Element/group_Currency_Premium/icn_Currency_Premium").position;
			}
			return base.dooberModel.PremiumGlassPosition;
		}

		private Vector3 GetStorageGlassPosition()
		{
			if (base.dooberModel.StorageGlassPosition == Vector3.zero)
			{
				base.dooberModel.StorageGlassPosition = hud.transform.Find("group_Store_Element/group_Storage/icn_Storage").position;
			}
			return base.dooberModel.StorageGlassPosition;
		}

		private Vector3 GetInspirationGlassPosition()
		{
			if (base.dooberModel.InspirationGlassPosition == Vector3.zero)
			{
				base.dooberModel.InspirationGlassPosition = hud.transform.Find("PointsPanel/XP_FunMeter/Fun_Meter/screen_InspirationPanel").position;
			}
			return base.dooberModel.InspirationGlassPosition;
		}

		private Vector3 GetDiscoGlassPosition()
		{
			if (base.dooberModel.DiscoBallGlassPosition == Vector3.zero)
			{
				base.dooberModel.DiscoBallGlassPosition = hud.transform.Find("panel_DiscoGlobe/dooberFlyUpPosition").position;
			}
			return base.dooberModel.DiscoBallGlassPosition;
		}

		private Vector3 GetTikiHutGlassPosition()
		{
			if (base.dooberModel.TikiBarWorldPosition == Vector3.zero)
			{
				base.dooberModel.TikiBarWorldPosition = GetBuildingGlassPosition(313);
			}
			return uiCamera.ViewportToWorldPoint(base.dooberModel.TikiBarWorldPosition);
		}

		private Vector3 GetStoreGlassPosition()
		{
			if (base.dooberModel.StoreGlassPosition == Vector3.zero)
			{
				base.dooberModel.StoreGlassPosition = hud.transform.Find("panel_BuildMenu/img_backing").position;
			}
			return base.dooberModel.StoreGlassPosition;
		}

		private Vector3 GetMinionLevelTokenGlassPosition()
		{
			if (base.dooberModel.TokenInfoHUDPosition == Vector3.zero)
			{
				RectTransform rectTransform = hud.transform.Find("panel_TokenInfo/img_Token").transform as RectTransform;
				Vector3 tokenInfoHUDPosition = new Vector3(rectTransform.position.x + 4f, rectTransform.position.y, rectTransform.position.z);
				base.dooberModel.TokenInfoHUDPosition = tokenInfoHUDPosition;
			}
			return base.dooberModel.TokenInfoHUDPosition;
		}

		private Vector3 GetBuildingGlassPosition(int buildingInstanceId)
		{
			GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER);
			BuildingManagerView component = instance.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(buildingInstanceId);
			return Camera.main.WorldToViewportPoint(buildingObject.transform.position);
		}

		private Vector3 GetMiscGlassPosition()
		{
			if (base.dooberModel.MiscGlassPosition == Vector3.zero)
			{
				base.dooberModel.MiscGlassPosition = hud.transform.Find("misc_DooberLocation").position;
			}
			return base.dooberModel.MiscGlassPosition;
		}

		private GameObject CreateTweenObject(Transform glassTransform)
		{
			Vector2 screenStartPosition = GetScreenStartPosition();
			GameObject original = ((type != DestinationType.MINION_LEVEL_TOKEN && type != DestinationType.MYSTERY_BOX && type != DestinationType.STORAGE_POPULATION_GOAL && type != DestinationType.TIMER_POPULATION_GOAL) ? (KampaiResources.Load("TweeningDoober") as GameObject) : (KampaiResources.Load("MysteryBox_TweeningDoober") as GameObject));
			GameObject gameObject = Object.Instantiate(original);
			gameObject.name = "ScreenTween Object";
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.zero;
			gameObject.transform.SetParent(glassTransform, false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, glassTransform.position.z);
			component.offsetMin = new Vector2(screenStartPosition.x - iconWidth / 2f, screenStartPosition.y - iconWidth / 2f);
			component.offsetMax = new Vector2(screenStartPosition.x + iconWidth / 2f, screenStartPosition.y + iconWidth / 2f);
			gameObject.transform.localScale = Vector3.one;
			return gameObject;
		}

		private Sprite GetIconFromDefinitionID(int id)
		{
			DisplayableDefinition displayableDefinition = definitionService.Get<DisplayableDefinition>(id);
			if (displayableDefinition != null)
			{
				return UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
			}
			return null;
		}

		private Sprite GetMaskFromDefinitionId(int id)
		{
			DisplayableDefinition displayableDefinition = definitionService.Get<DisplayableDefinition>(id);
			if (displayableDefinition != null)
			{
				return UIUtils.LoadSpriteFromPath(displayableDefinition.Mask);
			}
			return null;
		}
	}
}

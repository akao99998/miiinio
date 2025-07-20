using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public abstract class WorldToGlassView : strange.extensions.mediation.impl.View, IDefinitionsHotSwapHandler, IWorldToGlassView
	{
		public bool isHidden;

		public bool isForceHideEnabled;

		protected int m_trackedId;

		protected Vector3 UIOffset;

		protected IKampaiLogger logger;

		protected IPlayerService playerService;

		protected ILocalizationService localizationService;

		protected IPositionService positionService;

		protected ActionableObject targetObject;

		protected ICrossContextCapable gameContext;

		protected WorldToGlassUISettings m_Settings;

		protected Transform m_transform;

		protected WorldToGlassUIModal m_modal;

		public int TrackedId
		{
			get
			{
				return m_trackedId;
			}
		}

		public GameObject GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		protected abstract string UIName { get; }

		internal void Init(IPositionService positionService, ICrossContextCapable gameContext, IKampaiLogger logger, IPlayerService playerService, ILocalizationService localizationService)
		{
			this.positionService = positionService;
			this.playerService = playerService;
			this.localizationService = localizationService;
			this.logger = logger;
			this.gameContext = gameContext;
			m_transform = base.transform;
			m_modal = GetComponent<WorldToGlassUIModal>();
			m_Settings = m_modal.Settings;
			m_trackedId = m_Settings.TrackedId;
			UIOffset = Vector3.zero;
			BuildingManagerView component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetScaffoldingBuildingObject(m_trackedId) ?? component.GetBuildingObject(m_trackedId);
			if (buildingObject != null)
			{
				targetObject = buildingObject;
			}
			else
			{
				targetObject = ActionableObjectManagerView.GetFromAllObjects(m_trackedId);
			}
			base.name = string.Format("{0}-{1}", UIName, TrackedId);
			LoadModalData(m_modal);
			Hide();
		}

		public virtual void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			LoadModalData(m_modal);
		}

		protected abstract void LoadModalData(WorldToGlassUIModal modal);

		protected virtual void OnHide()
		{
			Image[] componentsInChildren = m_transform.GetComponentsInChildren<Image>(true);
			foreach (Image image in componentsInChildren)
			{
				image.enabled = false;
			}
		}

		protected void Hide()
		{
			if (!isHidden)
			{
				isHidden = true;
				OnHide();
			}
		}

		public virtual void SetForceHide(bool forceHide)
		{
			isForceHideEnabled = forceHide;
		}

		protected virtual void OnShow()
		{
			Image[] componentsInChildren = m_transform.GetComponentsInChildren<Image>(true);
			foreach (Image image in componentsInChildren)
			{
				image.enabled = true;
			}
		}

		protected void Show()
		{
			if (isHidden)
			{
				isHidden = false;
				OnShow();
			}
		}

		public virtual Vector3 GetIndicatorPosition()
		{
			BuildingObject buildingObject = targetObject as BuildingObject;
			if (buildingObject != null)
			{
				return buildingObject.IndicatorPosition;
			}
			CharacterObject characterObject = targetObject as CharacterObject;
			if (characterObject != null)
			{
				if (characterObject.DefinitionID == 70000)
				{
					BuildingManagerView component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerView>();
					BuildingObject buildingObject2 = component.GetScaffoldingBuildingObject(313) ?? component.GetBuildingObject(313);
					return buildingObject2.IndicatorPosition;
				}
				return characterObject.GetIndicatorPosition();
			}
			MasterPlanObject masterPlanObject = targetObject as MasterPlanObject;
			if (masterPlanObject != null)
			{
				UIOffset = GameConstants.UI.MASTER_PLAN_COOLDOWN_OFFSET;
				return masterPlanObject.GetIndicatorPosition();
			}
			return Vector3.zero;
		}

		internal virtual bool CanUpdate()
		{
			if (targetObject == null)
			{
				TargetObjectNullResponse();
				return false;
			}
			return true;
		}

		internal virtual void TargetObjectNullResponse()
		{
			logger.Warning("Removing UI id: {0} since the target object does not exist anymore!", m_trackedId);
			Hide();
		}

		internal virtual void OnUpdatePosition(PositionData positionData)
		{
			m_transform.position = positionData.WorldPositionInUI;
			m_transform.localPosition = VectorUtils.ZeroZ(m_transform.localPosition);
		}

		internal void LateUpdate()
		{
			if (!CanUpdate())
			{
				Hide();
				return;
			}
			PositionData positionData = positionService.GetPositionData(GetIndicatorPosition() + UIOffset);
			OnUpdatePosition(positionData);
			if (isForceHideEnabled)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}
	}
}

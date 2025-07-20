using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public abstract class AbstractWayFinderView : WorldToGlassView, IWayFinderView, IWorldToGlassView
	{
		public Signal RemoveWayFinderSignal = new Signal();

		public Signal UpdateWayFinderPrioritySignal = new Signal();

		public Signal SimulateClickSignal = new Signal();

		private bool _snappable;

		private bool isTargetObjectVisible;

		private bool isFTUEWayFinder;

		protected IZoomCameraModel zoomCameraModel;

		protected WayFinderDefinition wayFinderDefinition;

		protected ITikiBarService tikiBarService;

		protected Animator m_Animator;

		protected ButtonView m_GoToButton;

		protected KampaiImage m_CenterImage;

		private string m_CurrentIcon;

		protected Transform m_NoRotationTransform;

		protected Prestige m_Prestige;

		public Prestige Prestige
		{
			get
			{
				return m_Prestige;
			}
		}

		public bool ClickedOnce { get; set; }

		public bool AvoidsHUD { get; set; }

		public virtual bool Snappable
		{
			get
			{
				return _snappable;
			}
			set
			{
				_snappable = value;
				AvoidsHUD = value;
				if (_snappable)
				{
					base.transform.SetAsLastSibling();
				}
			}
		}

		protected abstract string WayFinderDefaultIcon { get; }

		internal void Init(IPositionService positionService, ICrossContextCapable gameContext, IKampaiLogger logger, IZoomCameraModel zoomCameraModel, ITikiBarService tikiBarService, IPlayerService playerService, ILocalizationService localizationService, IDefinitionService definitionService)
		{
			wayFinderDefinition = definitionService.Get<WayFinderDefinition>(1000008086);
			this.zoomCameraModel = zoomCameraModel;
			this.tikiBarService = tikiBarService;
			if (playerService.GetHighestFtueCompleted() < 9)
			{
				isFTUEWayFinder = true;
				Snappable = true;
			}
			Init(positionService, gameContext, logger, playerService, localizationService);
		}

		public override void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			wayFinderDefinition = definitionService.Get<WayFinderDefinition>(1000008086);
			base.OnDefinitionsHotSwap(definitionService);
		}

		protected virtual void OnLoadWayFinderModal(WayFinderModal wayFinderModal)
		{
		}

		protected override void LoadModalData(WorldToGlassUIModal modal)
		{
			WayFinderModal wayFinderModal = modal as WayFinderModal;
			if (wayFinderModal == null)
			{
				logger.Error("Way Finder Modal doesn't exist!");
				return;
			}
			m_Animator = wayFinderModal.Animator;
			m_GoToButton = wayFinderModal.GoToButton;
			m_Prestige = wayFinderModal.Prestige;
			m_CenterImage = wayFinderModal.SpecificModel.CenterImage;
			m_NoRotationTransform = wayFinderModal.SpecificModel.NoRotationTransform;
			wayFinderModal.GenericModel.gameObject.SetActive(false);
			wayFinderModal.SpecificModel.gameObject.SetActive(true);
			OnLoadWayFinderModal(wayFinderModal);
			Building byInstanceId = playerService.GetByInstanceId<Building>(m_trackedId);
			if (byInstanceId != null)
			{
				BuildingDefinition definition = byInstanceId.Definition;
				if (definition != null)
				{
					UIOffset = definition.QuestIconOffset;
				}
				if (definition.ID == 3015 && playerService.GetCountByDefinitionId(definition.ID) == 1)
				{
					isFTUEWayFinder = true;
					Snappable = true;
				}
			}
			UpdateIcon(WayFinderDefaultIcon);
			InitSubView();
		}

		protected virtual void InitSubView()
		{
		}

		internal virtual void Clear()
		{
		}

		public void SimulateClick()
		{
			if (!isHidden)
			{
				SimulateClickSignal.Dispatch();
			}
		}

		internal void UpdateIcon(string icon)
		{
			if (!(icon == m_CurrentIcon))
			{
				ClickedOnce = false;
				m_CurrentIcon = icon;
				m_CenterImage.maskSprite = KampaiResources.Load<Sprite>(icon);
			}
		}

		public bool IsTargetObjectVisible()
		{
			return isTargetObjectVisible;
		}

		protected abstract bool OnCanUpdate();

		internal override bool CanUpdate()
		{
			if (targetObject == null && TrackedId != 1000008087)
			{
				RemoveWayFinderSignal.Dispatch();
				return false;
			}
			if (zoomCameraModel.ZoomInProgress)
			{
				return false;
			}
			if (!OnCanUpdate())
			{
				return false;
			}
			return true;
		}

		internal override void OnUpdatePosition(PositionData positionData)
		{
			if (Snappable || AvoidsHUD)
			{
				SnappablePositionData snappablePositionData = positionService.GetSnappablePositionData(positionData, ViewportBoundary.FULLSCREEN, true);
				positionData = new PositionData(snappablePositionData);
				if (Snappable)
				{
					m_transform.position = snappablePositionData.ClampedWorldPositionInUI;
				}
				else
				{
					m_transform.position = positionData.WorldPositionInUI;
				}
			}
			else
			{
				m_transform.position = positionData.WorldPositionInUI;
			}
			m_transform.localPosition = VectorUtils.ZeroZ(m_transform.localPosition);
			if (positionData.IsVisible)
			{
				OnVisible();
			}
			else
			{
				OnInvisible(positionData.ViewportDirectionFromCenter);
			}
			m_NoRotationTransform.rotation = Quaternion.identity;
		}

		protected virtual void OnVisible()
		{
			if (!ClickedOnce && isFTUEWayFinder)
			{
				m_Animator.Play("Pulse");
			}
			else
			{
				m_Animator.Play("Idle");
			}
			m_transform.rotation = Quaternion.identity;
			isTargetObjectVisible = true;
		}

		protected virtual void OnInvisible(Vector3 direction)
		{
			isTargetObjectVisible = false;
			if (Snappable)
			{
				Quaternion rotation = Quaternion.LookRotation(direction, Vector3.forward);
				rotation.x = 0f;
				rotation.y = 0f;
				m_transform.rotation = rotation;
			}
			else
			{
				m_transform.rotation = Quaternion.identity;
			}
			m_Animator.Play("Idle");
		}
	}
}

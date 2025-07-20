using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public abstract class BuildingObject : BuildingDefinitionObject
	{
		private Signal<int, MinionTaskInfo> updateSignal;

		private readonly Signal stopBuildingAudioInIdleStateSignal = new Signal();

		protected Collider minColliderY;

		private Renderer maxRendererY;

		private Bounds combinedRendererBounds;

		private Color highlightColor = Color.grey;

		private Animation cachedAnimation;

		protected IDefinitionService definitionService;

		private Signal networkOpenSignal;

		private Signal netowrkCloseSignal;

		public Vector3 IndicatorPosition
		{
			get
			{
				return GetIndicatorPosition(false);
			}
		}

		public Vector3 Center
		{
			get
			{
				return GetCenter(true);
			}
		}

		public Vector3 ZoomCenter
		{
			get
			{
				return GetZoomCenterPosition();
			}
		}

		public Signal StopBuildingAudioInIdleStateSignal
		{
			get
			{
				return stopBuildingAudioInIdleStateSignal;
			}
		}

		public GameObject MinionPartyDecorations { get; set; }

		protected Signal<int, MinionTaskInfo> GetUpdateSignal()
		{
			if (updateSignal == null)
			{
				updateSignal = base.transform.parent.GetComponent<BuildingManagerView>().updateMinionSignal;
			}
			return updateSignal;
		}

		internal virtual void Highlight(bool enabled)
		{
			Color materialColor = ((!enabled) ? Color.white : highlightColor);
			SetMaterialColor(materialColor);
		}

		internal virtual void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.logger = logger;
			this.definitionService = definitionService;
			ID = building.ID;
			Init(building.Definition, definitionService);
			UpdateColliderState(building.State);
			cachedAnimation = GetComponent<Animation>();
			if (base.colliders.Length > 0)
			{
				minColliderY = base.colliders[0];
				Collider[] array = base.colliders;
				foreach (Collider collider in array)
				{
					if (collider.bounds.min.y < minColliderY.bounds.min.y)
					{
						minColliderY = collider;
					}
				}
			}
			if (base.objectRenderers.Length <= 0)
			{
				return;
			}
			maxRendererY = base.objectRenderers[0];
			Renderer[] array2 = base.objectRenderers;
			foreach (Renderer renderer in array2)
			{
				if (renderer.bounds.max.y > maxRendererY.bounds.max.y)
				{
					maxRendererY = renderer;
				}
				combinedRendererBounds.Encapsulate(renderer.bounds);
			}
		}

		public virtual void UpdateColliderState(BuildingState state)
		{
			base.IsInteractable = state != BuildingState.Disabled;
			UpdateColliders(base.IsInteractable);
		}

		private Vector3 GetCenter(bool centerY)
		{
			Vector3 position = base.transform.position;
			if (minColliderY != null)
			{
				return new Vector3(minColliderY.bounds.center.x, (!centerY) ? minColliderY.bounds.max.y : minColliderY.bounds.center.y, minColliderY.bounds.center.z);
			}
			return new Vector3(position.x, 0f, position.z);
		}

		protected virtual Vector3 GetIndicatorPosition(bool centerY)
		{
			if (maxRendererY != null)
			{
				return new Vector3(maxRendererY.bounds.center.x, (!centerY) ? maxRendererY.bounds.max.y : maxRendererY.bounds.center.y, maxRendererY.bounds.center.z);
			}
			return GetCenter(centerY);
		}

		protected virtual Vector3 GetZoomCenterPosition()
		{
			Vector3 position = base.transform.position;
			if (combinedRendererBounds.extents.sqrMagnitude > 0f)
			{
				return new Vector3(position.x + combinedRendererBounds.center.x, combinedRendererBounds.center.y, position.z + combinedRendererBounds.center.z);
			}
			return GetCenter(true);
		}

		public void SetUpWifiListeners(Signal openSignal, Signal closeSignal)
		{
			networkOpenSignal = openSignal;
			netowrkCloseSignal = closeSignal;
			networkOpenSignal.AddListener(Hide);
			netowrkCloseSignal.AddListener(Show);
		}

		protected virtual void Hide()
		{
			base.gameObject.SetActive(false);
		}

		protected virtual void Show()
		{
			base.gameObject.SetActive(true);
		}

		public virtual void Bounce()
		{
			if (cachedAnimation != null)
			{
				cachedAnimation.Play();
			}
		}

		public virtual void Cleanup()
		{
			if (networkOpenSignal != null)
			{
				networkOpenSignal.RemoveListener(Hide);
			}
			if (netowrkCloseSignal != null)
			{
				netowrkCloseSignal.RemoveListener(Show);
			}
		}

		public override bool CanFadeGFX()
		{
			return true;
		}

		public override bool CanFadeSFX()
		{
			return true;
		}
	}
}

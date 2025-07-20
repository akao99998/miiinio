using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public abstract class CharacterObject : ActionableObject
	{
		private GameObject blobShadow;

		private List<Renderer> renderers;

		protected RuntimeAnimatorController cachedDefaultController;

		protected RuntimeAnimatorController cachedRuntimeController;

		protected bool animatorControllersAreEqual;

		private Renderer defaultRenderer;

		private Transform pelvisTransform;

		public bool IsSeatedInTikiBar { get; set; }

		public virtual void Init(Character character, IKampaiLogger logger)
		{
			base.Init();
			ID = character.ID;
			base.name = character.Name;
			base.DefinitionID = character.Definition.ID;
			base.logger = logger;
			SetName(character);
			animators = new List<Animator>(base.gameObject.GetComponentsInChildren<Animator>());
			renderers = new List<Renderer>(3);
			RefreshRenderers();
			defaultRenderer = base.gameObject.GetComponent<Renderer>();
			GameObject gameObject = base.transform.FindChild(GetRootJointPath(character)).gameObject;
			BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
			if (boxCollider == null)
			{
				boxCollider = gameObject.AddComponent<BoxCollider>();
			}
			boxCollider.center = new Vector3(0f, 0.35f, 0f);
			boxCollider.size = new Vector3(0.75f, 1f, 0.75f);
			if (base.DefinitionID == 70000)
			{
				boxCollider.size = new Vector3(0.75f, 5f, 0.75f);
			}
			pelvisTransform = gameObject.transform;
			InitProps();
			IsSeatedInTikiBar = false;
		}

		protected virtual string GetRootJointPath(Character character)
		{
			return "minion:ROOT/minion:pelvis_jnt";
		}

		protected virtual void SetName(Character character)
		{
			if (base.gameObject.name.Length == 0)
			{
				base.gameObject.name = string.Format("Minion_{0}", character.ID);
			}
		}

		public override void EnableRenderers(bool enabled)
		{
			foreach (Renderer renderer in renderers)
			{
				renderer.enabled = enabled;
			}
			EnableBlobShadow(enabled);
		}

		public virtual void SetBlobShadow(GameObject shadow)
		{
			blobShadow = shadow;
		}

		public virtual void EnableBlobShadow(bool enabled)
		{
			if (blobShadow != null)
			{
				blobShadow.GetComponent<Renderer>().enabled = enabled;
			}
		}

		public virtual void UpdateBlobShadowPosition()
		{
			if (blobShadow != null)
			{
				blobShadow.GetComponent<MinionBlobShadowView>().ManualUpdate();
			}
		}

		public override void LateUpdate()
		{
			base.LateUpdate();
			if (animators != null && (animators[0].runtimeAnimatorController != cachedRuntimeController || defaultController != cachedDefaultController))
			{
				cachedRuntimeController = animators[0].runtimeAnimatorController;
				cachedDefaultController = defaultController;
				if ((bool)cachedRuntimeController)
				{
					animatorControllersAreEqual = cachedRuntimeController.name.Equals(defaultController.name);
				}
				else
				{
					animatorControllersAreEqual = false;
				}
			}
		}

		public virtual void SetMuteStatus(bool muteStatus)
		{
			ExecuteAction(new MuteAction(this, muteStatus, logger));
		}

		public virtual void ResetRootPosition()
		{
			Transform transform = base.gameObject.FindChild("minion:ROOT").transform;
			Transform transform2 = base.gameObject.transform;
			transform2.position = VectorUtils.ZeroY(transform2.position + transform.localPosition);
			transform2.rotation = Quaternion.Euler(VectorUtils.ZeroXZ(transform2.rotation.eulerAngles + transform.localRotation.eulerAngles));
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.Euler(Vector3.zero);
		}

		public virtual void MoveToPelvis()
		{
			Transform transform = base.gameObject.transform;
			Transform transform2 = base.gameObject.FindChild("minion:pelvis_jnt").transform;
			transform.position = VectorUtils.ZeroY(transform2.position);
			transform.rotation = Quaternion.Euler(VectorUtils.ZeroXZ(transform2.rotation.eulerAngles));
		}

		public void ResetAnimationController()
		{
			if (animators.Count == 0 || animators[0].runtimeAnimatorController == defaultController)
			{
				return;
			}
			List<Animator>.Enumerator enumerator = animators.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SetAnimController(defaultController);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			vfxTrigger = null;
		}

		public void setLocation(Vector3 position)
		{
			base.transform.position = position;
		}

		public void setRotation(Vector3 rotation)
		{
			base.transform.localEulerAngles = rotation;
		}

		public virtual Object GetBlobShadow()
		{
			return blobShadow;
		}

		public virtual Vector3 GetIndicatorPosition()
		{
			float y = 2f;
			if ((bool)defaultRenderer)
			{
				y = defaultRenderer.bounds.max.y;
			}
			Vector3 vector = ((!(blobShadow != null)) ? (pelvisTransform.position - new Vector3(0f, 0.8f, 0f)) : blobShadow.transform.position);
			return vector + new Vector3(0f, y, 0f);
		}

		public void RefreshRenderers()
		{
			renderers.Clear();
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (renderer != null && !renderer.gameObject.name.StartsWith("selectIcon"))
				{
					renderers.Add(renderer);
				}
			}
		}

		protected void EnableCollider(bool enable)
		{
			Collider component = pelvisTransform.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = enable;
			}
		}
	}
}

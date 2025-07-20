using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;

namespace Kampai.Game.View
{
	public class DCNBuildingObjectView : BuildingObject, IView
	{
		private MeshRenderer openScreen;

		private bool open;

		private bool _requiresContext = true;

		protected bool registerWithContext = true;

		private IContext currentContext;

		public bool requiresContext
		{
			get
			{
				return _requiresContext;
			}
			set
			{
				_requiresContext = value;
			}
		}

		public bool registeredWithContext { get; set; }

		public virtual bool autoRegisterWithContext
		{
			get
			{
				return registerWithContext;
			}
			set
			{
				registerWithContext = value;
			}
		}

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			GameObject gameObject = base.gameObject.FindChild("Unique_DCN:Unique_DCN_OpenTarp_Mesh");
			openScreen = gameObject.GetComponent<MeshRenderer>();
			HideScreen();
		}

		internal void ShowScreen()
		{
			openScreen.enabled = true;
			open = true;
		}

		internal void HideScreen()
		{
			openScreen.enabled = false;
			open = false;
		}

		internal bool ScreenIsOpen()
		{
			return open;
		}

		protected void Awake()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, false, ref currentContext);
			}
		}

		protected void Start()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, true, ref currentContext);
			}
		}

		protected void OnDestroy()
		{
			KampaiView.BubbleToContext(this, false, false, ref currentContext);
		}
	}
}
